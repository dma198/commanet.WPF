using System;
using System.Collections.Generic;


namespace commanet.WPF
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOfAttribute : Attribute
    {

        public List<String> Depends {get; } = new List<string>(); 
        public DependsOfAttribute(params string[] depends)
        {
            foreach(var d in depends)
                Depends.Add(d);
        }

    }
}
