using System;
using System.Linq;
using System.Windows;

namespace commanet.WPF
{
    public class View : Window
    {
        public View()    
        {
            var vmname = GetType().Name+"ViewModel";
            
            foreach (var t in
                (from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                 from t in a.GetTypes()
                 let cname = t.Name
                 where cname==vmname
                 select t).ToList())
            {
               DataContext = Activator.CreateInstance(t);
            }

        }

    }
}
