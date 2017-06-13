using System;
using System.ComponentModel;
using System.Linq;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public class TypedCalculatorViewModel
    {
        private readonly string _description;

        public TypedCalculatorViewModel(Type type)
        {
            Type = type;
            var attributes = (DescriptionAttribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            _description = attributes.Any()
                               ? attributes.First()
                                     .Description
                               : type.Name;
        }

        public string Name => Type.Name;

        public Type Type { get; }

        public override string ToString()
        {
            return _description;
        }
    }
}