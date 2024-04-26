using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLoadRunner
{
    public class ConsoleWriteContext
    {
        #region Variables

        public string LineHeader;
        public bool StayOnSameLine = true;

        static Object ConsoleLock = new object();

        int _X;
        int _Y;
        bool _hasBeenUsed = false;
        int _workingAnimationIndex;
        List<char> _workingAnimationCharacters = new List<char>();
        DateTime _createdTime = DateTime.Now;

        #endregion

        #region Ctor

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="stayOnSameLine">Keeps output on 1 line using word wrapping</param>
        /// <param name="lineHeader">Text to keep at left of each line</param>
        public ConsoleWriteContext(bool stayOnSameLine, string lineHeader)
        {
            LineHeader = lineHeader + " ";
            StayOnSameLine = stayOnSameLine;

            _workingAnimationCharacters.Add('-');
            _workingAnimationCharacters.Add('\\');
            _workingAnimationCharacters.Add('|');
            _workingAnimationCharacters.Add('/');

            lock (ConsoleLock)
            {
                if (Console.CursorLeft != 0)
                {
                    Console.WriteLine();
                }

                _X = Console.CursorLeft;
                _Y = Console.CursorTop;
                ClearLine();
                // we own the current line so lets create a fresh one for the live console
                Console.WriteLine();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes from the start of the current line after clearing it
        /// </summary>
        /// <param name="message">String to write</param>
        public void WriteStart(string message)
        {
            lock (ConsoleLock)
            {
                ClearLine();

                _X = 0;

                if (!string.IsNullOrEmpty(LineHeader))
                {
                    WriteContinue(LineHeader);
                }

                WriteXY(message, _X, _Y);
                _X = _X + message.Length;
            }
        }

        /// <summary>
        /// Writes from the begining of a newly created line
        /// </summary>
        /// <param name="message"></param>
        public void WriteNewLine(string message)
        {
            lock (ConsoleLock)
            {
                _Y = Console.CursorTop;
                Console.WriteLine();
                _X = 0;

                if (!string.IsNullOrEmpty(LineHeader)) { WriteContinue(LineHeader); }

                WriteXY(message, _X, _Y);
                _X = _X + message.Length;
            }
        }

        /// <summary>
        /// Writes the message and continues the current cursor position
        /// </summary>
        /// <param name="message">String to write</param>
        public void WriteContinue(string message)
        {
            lock (ConsoleLock)
            {
                // handle writing past end of line buffer                             
                if ((_X + message.Length) >= Console.WindowWidth)
                {
                    _X = 0;

                    if (StayOnSameLine)
                    {
                        ClearLine();
                    }
                    else
                    {
                        // give this context current live console line
                        _Y = Console.CursorTop;
                        ClearLine();
                        // then move live console on a row
                        Console.WriteLine();
                    }
                }
                WriteXY(message, _X, _Y);
                _X = _X + message.Length;
            }
        }

        /// <summary>
        /// Writes the message and keeps the current cursor position where it was
        /// </summary>
        /// <param name="message">String to write</param>
        public void WriteAtCurrent(string message)
        {
            lock (ConsoleLock)
            {
                WriteXY(message, _X, _Y);
            }
        }

        public void WritePercentage(int pc, bool includeAnimation, bool includeProgressDots)
        {
            lock (ConsoleLock)
            {

                ClearLine();

                string outputString = "";

                // check limits
                if (pc > 100) pc = 100;
                if (pc < 0) pc = 0;

                outputString += pc.ToString() + "% ";

                if (includeAnimation)
                {
                    if (pc < 100)
                    {
                        outputString += (GetNextWorkingAnimChar().ToString() + " ");
                    }
                    else
                    {
                        outputString += " ";
                    }
                }

                if (includeProgressDots)
                {
                    outputString += "[";
                    for (int x = 10; x <= 100; x = x + 10)
                    {
                        //int upperBound = x * 10;
                        if (x <= pc)
                        {
                            outputString += "x";
                        }
                        else
                        {
                            outputString += " ";
                        }
                    }

                    outputString += "]";
                }
                WriteContinue(outputString);
            }
        }
        /// <summary>
        /// Returns the next character in the busy animation sequence
        /// </summary>
        /// <returns></returns>
        public char GetNextWorkingAnimChar()
        {
            _workingAnimationIndex++;
            if (_workingAnimationIndex >= _workingAnimationCharacters.Count) _workingAnimationIndex = 0;
            return _workingAnimationCharacters[_workingAnimationIndex];
        }

        /// <summary>
        /// Writes the next character in the busy animation sequence to the current cursor position
        /// </summary>
        public void WriteWorkingAnimation()
        {
            lock (ConsoleLock)
            {
                if (_X == 0 && (!string.IsNullOrEmpty(LineHeader))) WriteContinue(LineHeader);
                WriteAtCurrent(GetNextWorkingAnimChar().ToString());
            }
        }

        public TimeSpan GetElapsedTime()
        {
            return DateTime.Now - _createdTime;
        }

        public void WriteElapsedTime(ElapsedTimeUnit unit, bool showAnimation)
        {
            var ts = GetElapsedTime();

            lock (ConsoleLock)
            {
                if (_X == 0 && (!string.IsNullOrEmpty(LineHeader))) WriteContinue(LineHeader);

                string outputString = "";
                if (showAnimation)
                {
                    outputString += " " + GetNextWorkingAnimChar();
                }

                if (unit == ElapsedTimeUnit.Ms)
                {
                    outputString += ts.TotalMilliseconds.ToString("N0") + "ms";
                }
                else if (unit == ElapsedTimeUnit.Second)
                {
                    outputString += ts.TotalSeconds.ToString("N0") + "s";
                }
                else
                {
                    outputString += ts.TotalMinutes + "m";
                }

                WriteAtCurrent(outputString);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Writes the string while handling new lines
        /// </summary>
        /// <param name="message">string to write</param>
        /// <param name="x">x cursor position</param>
        /// <param name="y">y cursor position</param>
        void WriteXY(string message, int x, int y)
        {
            lock (ConsoleLock)
            {
                int currentLeft = Console.CursorLeft;
                int currentTop = Console.CursorTop;

                Console.CursorVisible = false;
                Console.SetCursorPosition(x, y);
                Console.Write(message);
                Console.SetCursorPosition(currentLeft, currentTop);
                Console.CursorVisible = true;
            }
        }

        /// <summary>
        /// Clears the current line and preserves header if present
        /// </summary>
        void ClearLine()
        {
            lock (ConsoleLock)
            {
                int beginPosition = 0;
                string outputString = "";
                if (!string.IsNullOrEmpty(LineHeader))
                {
                    outputString += LineHeader;
                }


                for (int x = outputString.Length; x < Console.WindowWidth; x++)
                {
                    outputString += " ";
                }
                WriteXY(outputString, 0, _Y);
                _X = LineHeader.Length;
            }
        }

        #endregion


    }
    public enum ElapsedTimeUnit
    {
        Ms, Second, Minute
    }
}
