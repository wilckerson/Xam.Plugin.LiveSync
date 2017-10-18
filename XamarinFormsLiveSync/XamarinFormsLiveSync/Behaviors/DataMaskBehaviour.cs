using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsLiveSync.Behaviors
{
    public class DataMaskBehavior : Behavior<Entry>
    {
        CultureInfo culture = new CultureInfo("pt-BR"); //CultureInfo.CurrentCulture;

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            Entry control = sender as Entry;

            if (args.NewTextValue == args.OldTextValue) { return; }

            string numbersOnly = Regex.Replace(control.Text ?? "", @"[^\d]", "");

            StringBuilder sb = new StringBuilder();
            sb.Append(numbersOnly.Substring(0, Math.Min(2, numbersOnly.Length)));

            if (numbersOnly.Length > 2)
            {
                sb.Append("/");
                sb.Append(numbersOnly.Substring(2, Math.Min(2, numbersOnly.Length - 2)));
            }

            if (numbersOnly.Length > 4)
            {
                sb.Append("/");
                sb.Append(numbersOnly.Substring(4, Math.Min(4, numbersOnly.Length - 4)));
            }

            //Atribuindo o resultado da mascara
            string result = sb.ToString();

            //Limpa tudo se for uma data inválida
            if (numbersOnly.Length == 8 && !DateTime.TryParse(result, culture, DateTimeStyles.None, out DateTime r))
            {
                result = string.Empty;
            }

            control.Text = result;
        }
    }
}
