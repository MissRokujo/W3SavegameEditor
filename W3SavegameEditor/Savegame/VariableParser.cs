﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using W3SavegameEditor.Savegame.VariableParsers;
using W3SavegameEditor.Savegame.Variables;

namespace W3SavegameEditor.Savegame
{
    public class VariableParser
    {
        private readonly string[] _names;
        private readonly Dictionary<string, VariableParserBase> _parsers;

        public VariableParser(string[] names)
        {
            _names = names;
            _parsers = new Dictionary<string, VariableParserBase>();
        }

        public void RegisterParsers(IEnumerable<VariableParserBase> parsers)
        {
            foreach (var parser in parsers)
            {
                _parsers[parser.MagicNumber] = parser;
            }
        }
        
        public VariableBase Parse(BinaryReader reader, int size)
        {
            string magicNumber = reader.PeekString(2);

            VariableParserBase parser;
            if (_parsers.TryGetValue(magicNumber, out parser))
            {
                parser.Verify(reader);
                var variable = parser.Parse(reader, size);
                variable.ResolveNames(_names);
                return variable;
            }
            else
            {
                string hexMagicNumber = BitConverter.ToString(Encoding.ASCII.GetBytes(magicNumber));

                Debug.WriteLine(
                    "Failed to parse {0} bytes of data at {1}. Magic number was {2}",
                    size,
                    reader.BaseStream.Position,
                    hexMagicNumber);
                return Variable.None;
            }
        }
    }

}
