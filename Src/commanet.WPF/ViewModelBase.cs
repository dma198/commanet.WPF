using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace commanet.WPF
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        private Dictionary<string,List<string>> depends = new Dictionary<string, List<string>>();

        public ViewModelBase()
        {
            var t = GetType();
            foreach(var pi in t.GetProperties())
            {
                #region Bind Command                
                if(pi.Name.EndsWith("Command")  && pi.PropertyType==typeof(Command))
                {
                    var cname=pi.Name.Substring(0,pi.Name.Length-7);                   
                    MethodInfo? exec=null;
                    MethodInfo? canexec=null;
                    MethodInfo? beforeexec=null;
                    MethodInfo? afterexec=null;
                    MethodInfo? execexcept=null;
                    foreach(var mi in t.GetMethods())
                    {
                        if(mi.Name==cname+"Execute" && mi.GetParameters().Length==1 && 
                           mi.GetParameters()[0].ParameterType==typeof(object))
                            exec=mi;    
                        else if(mi.Name==cname+"CanExecute" && mi.GetParameters().Length==1 && 
                           mi.GetParameters()[0].ParameterType==typeof(object) &&
                           mi.ReturnType == typeof(bool))
                            canexec=mi;   
                        else if(mi.Name==cname+"BeforeExecute" && mi.GetParameters().Length==0 && 
                           mi.ReturnType == typeof(void))
                            beforeexec=mi;                              
                        else if(mi.Name==cname+"AfterExecute" && mi.GetParameters().Length==0 && 
                           mi.ReturnType == typeof(void))
                            afterexec=mi;                              
                        else if(mi.Name==cname+"ExecuteException" && mi.GetParameters().Length==1 &&
                           mi.GetParameters()[0].ParameterType==typeof(Exception) && 
                           mi.ReturnType == typeof(void))
                            execexcept=mi;  
                        if(exec!=null && canexec!=null && beforeexec!=null && afterexec!=null && execexcept!=null)
                            break;  
                    };
                    Action<object?>? aexec=null;
                    Func<object,bool>? acanexec=null;
                    Action? abeforeexec=null;
                    Action? aafterexec=null;
                    Action<Exception>? aexecexcept=null;
                    var errfmt = "Can't bind command handler {0}. Error:\n{1}";
                    var errcap = "Error";
                    
                    try
                    {
                        aexec = exec!=null ? (Action<object?>)exec.CreateDelegate(typeof(Action<object>),this) : null;
                    }
                    catch(Exception ex)
                    {
                        if(exec?.Name != null)
                            MessageBox.Show(string.Format(errfmt,exec.Name,ex.Message),errcap,MessageBoxButton.OK,MessageBoxImage.Error);
                    }

                    try
                    {
                        acanexec = canexec!=null ? (Func<object,bool>)canexec.CreateDelegate(typeof(Func<object,bool>),this) : null;                       
                    }
                    catch(Exception ex)
                    {
                        if(canexec?.Name !=null)
                            MessageBox.Show(string.Format(errfmt,canexec.Name,ex.Message),errcap,MessageBoxButton.OK,MessageBoxImage.Error);
                    }

                    try
                    {
                        abeforeexec = beforeexec!=null ? (Action)beforeexec.CreateDelegate(typeof(Action),this) : null;                       
                    }
                    catch(Exception ex)
                    {
                        if(beforeexec?.Name !=null)
                            MessageBox.Show(string.Format(errfmt,beforeexec.Name,ex.Message),errcap,MessageBoxButton.OK,MessageBoxImage.Error);
                    }

                    try
                    {
                        aafterexec = afterexec!=null ? (Action)afterexec.CreateDelegate(typeof(Action),this) : null;                       
                    }
                    catch(Exception ex)
                    {
                        if(afterexec?.Name !=null)
                            MessageBox.Show(string.Format(errfmt,afterexec.Name,ex.Message),errcap,MessageBoxButton.OK,MessageBoxImage.Error);
                    }
                    try
                    {
                        aexecexcept = execexcept!=null ? (Action<Exception>)execexcept.CreateDelegate(typeof(Action<Exception>),this) : null;                       
                    }
                    catch(Exception ex)
                    {
                        if(execexcept?.Name !=null)
                            MessageBox.Show(string.Format(errfmt,execexcept.Name,ex.Message),errcap,MessageBoxButton.OK,MessageBoxImage.Error);
                    }


                    pi.SetValue(this,new Command(aexec,acanexec,abeforeexec,aafterexec,aexecexcept));
                }
                #endregion

                #region Bind Dependent Properties
                var att  = pi.GetCustomAttribute<DependsOfAttribute>();
                if(att!=null)
                {
                    foreach(var dpname in att.Depends)
                    {
                        var dp= t.GetProperty(dpname);
                        if(dp!=null)
                        {
                          List<string> dps;
                          if(depends.ContainsKey(dp.Name)) dps =  depends[dp.Name];
                          else{
                            dps = new List<string>(); 
                            depends.Add(dp.Name,dps);   
                          } 

                          // Validate for circular dependency references  
                          var valid = true;  
                          if(depends.ContainsKey(pi.Name))
                          {
                              if(depends[pi.Name].Contains(dp.Name))
                              {
                                MessageBox.Show(string.Format("ViewModel {0} Properties {1} and {2} contains circular DependsOf(...) references",
                                        this.GetType().Name,  pi.Name,dp.Name),"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                                valid = false;        
                              }
                          }  
                          if(valid)dps.Add(pi.Name);
                        }
                    }
                }
                #endregion
            }           

        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        private void NotifyDependend(string propertyName,List<string>? alreadyNotified=null)
        {
            if(depends.ContainsKey(propertyName))
            {
                if(alreadyNotified==null)alreadyNotified=new List<string>();
                foreach(var dp in depends[propertyName])
                {
                   if(!alreadyNotified.Contains(dp))
                   {
                    NotifyPropertyChanged(dp);
                    alreadyNotified.Add(dp);          
                    NotifyDependend(dp,alreadyNotified);
                   }
                }
            }
        }        

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            NotifyDependend(propertyName);
        }

        public void SetAndNotify<T>(T src, ref T dest, [CallerMemberName]string propertyName = "")

        {
            dest = src;
            NotifyPropertyChanged(propertyName);
            NotifyDependend(propertyName);
        }

        public void SetAndNotify<T>(T src, [CallerMemberName]string propertyName = "")

        {
            var fname=char.ToLower(propertyName[0])+propertyName.Substring(1);
            var fi = this.GetType().GetField(fname,BindingFlags.NonPublic | BindingFlags.Instance);
            if(fi!=null)
            {
                fi.SetValue(this,src);
                NotifyPropertyChanged(propertyName);
            }
            NotifyDependend(propertyName);
        } 

        public void Notify(params string[] properties)
        {
            foreach(var p in properties)
            {
               if(p!=null)
                 NotifyPropertyChanged(p);
            }
        }       


    }
}
