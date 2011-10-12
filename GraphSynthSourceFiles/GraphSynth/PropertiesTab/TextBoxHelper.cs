using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphSynth.UI
{
    public static class TextBoxHelper
    {
        public static Boolean CanEvalNumber(TextBox tb, KeyEventArgs e)
        {
            var temp =
                ((e.Key != Key.Space) && (e.Key != Key.OemComma) && (e.Key != Key.Left)
                 && (e.Key != Key.Right) && (e.Key != Key.Up) && (e.Key != Key.OemPeriod));

            if (temp)
            {
                if (tb.CaretIndex < 2) return true;
                else return !(tb.Text.Substring(tb.CaretIndex - 2, 2).Equals(".0"));
            }
            else return false;
        }


        public static Boolean CanEvalString(KeyEventArgs e)
        {
            return
                ((e.Key != Key.Space) && (e.Key != Key.OemComma) && (e.Key != Key.Left)
                 && (e.Key != Key.Right) && (e.Key != Key.Up));
        }

        public static void SetCaret(TextBox tb, int caretIndex, int origLength)
        {
            var pos = caretIndex + (tb.Text.Length - origLength);
            if (pos >= 0) tb.CaretIndex = pos;
            else tb.CaretIndex = 0;
        }
    }
}