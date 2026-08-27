using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    class Parser
    {
        private List<Token> TokenList = new();
        private int Cursor;

        public void Set(List<Token> tokenList)
        {
            TokenList = tokenList;
            Cursor = 0;
        }

        public Data StartParsing()
        {
            if (TokenList.Count == 0)
                throw new Exception("No tokens to parse.");

            return Current.Value switch
            {
                "Frequenza" => ParseRadioWave(),
                "ADC" => ParseBattery(),
                "ERROR" => ParseError(),
                _ => throw new Exception(
                    $"Unexpected data type: {Current.Value}")
            };
        }

        private Token Current => TokenList[Cursor];

        private bool AtEnd => Cursor >= TokenList.Count;

        private Token Advance()
        {
            return TokenList[Cursor++];
        }

        private void Expect(TokenType type)
        {
            if (AtEnd || Current.Type != type)
            {
                throw new Exception(
                    $"Expected {type}, found " +
                    (AtEnd ? "end of tokens." : $"{Current.Type}."));
            }

            Advance();
        }

        private string ReadValue(string name, TokenType type)
        {
            var sb = new StringBuilder();

            while (!AtEnd && Current.Type == TokenType.Identifier)
            {
                if (sb.Length > 0)
                    sb.Append(' ');

                sb.Append(Advance().Value);
            }

            string actualName = sb.ToString();

            if (actualName != name)
            {
                throw new Exception($"Expected '{name}', found '{actualName}'.");
            }

            Expect(TokenType.Colon);

            if (AtEnd || Current.Type != type)
            {
                throw new Exception(
                    $"Expected {type} for '{name}', found " +
                    (AtEnd ? "end of tokens." : $"{Current.Type}."));
            }

            return Advance().Value;
        }

        private RadioWave ParseRadioWave()
        {
            RadioWave radioWave = new RadioWave
            {
                Frequency = int.Parse(
                    ReadValue("Frequenza", TokenType.IntValue)
                ),

                Amplitude = int.Parse(
                    ReadValue("Ampiezza", TokenType.IntValue)
                ),

                Duration = long.Parse(
                    ReadValue("Durata", TokenType.IntValue)
                ),

                Timestamp = int.Parse(
                    ReadValue("Timestamp", TokenType.IntValue)
                ),

                Type = int.Parse(
                    ReadValue("Type", TokenType.IntValue)
                )
            };

            DebugLogger.Info(
                "Parser",
                $"RadioWave parsed: " +
                $"Frequency={radioWave.Frequency}, " +
                $"Amplitude={radioWave.Amplitude}, " +
                $"Duration={radioWave.Duration}, " +
                $"Timestamp={radioWave.Timestamp}, " +
                $"Type={radioWave.Type}"
            );

            return radioWave;
        }

        private Battery ParseBattery()
        {
            Battery battery = new Battery
            {
                ADC = float.Parse(
                    ReadValue("ADC Battery", TokenType.FloatValue)
                ),

                VBattery = float.Parse(
                    ReadValue("Battery", TokenType.FloatValue)
                ),

                Charge = float.Parse(
                    ReadValue("Charge", TokenType.FloatValue)
                )
            };

            DebugLogger.Info(
                "Parser",
                $"Battery parsed: " +
                $"ADC={battery.ADC}, " +
                $"VBattery={battery.VBattery}, " +
                $"Charge={battery.Charge}"
            );

            return battery;
        }

        private Error ParseError()
        {
            if (Current.Value != "ERROR")
            {
                throw new Exception(
                    $"Expected 'ERROR', found '{Current.Value}'.");
            }

            Advance();
            Expect(TokenType.Colon);

            var sb = new StringBuilder();

            while (AtEnd || Current.Type != TokenType.Dot)
            {
                if (AtEnd)
                {
                    throw new Exception(
                        "Unexpected end of tokens while parsing ERROR message: missing terminating '.'.");
                }

                if (sb.Length > 0)
                sb.Append(' ');

                sb.Append(Advance().Value);
            }

            Advance(); 

            Error error = new Error
            {
                Type = sb.ToString()
            };

            DebugLogger.Info("Parser", $"Error parsed: Type={error.Type}");

            return error;
        }
    }
}