
commanet.WPF 
--------------------

Simplify routine operations when works with WPF.
Target is decrease amount of "garbage decoration" meaningless for buisness logic copy/paste code - usual source of mistakes.

In standard MVVM adding button: 

1. Add Button in XAML
2. In ViewModel: Add Command
3. In ViewModel: Create handler methods OnExecute (and CanExecute if needs)
4. In ViewModel: In constructor create  Command instance with passing handlers via parameters

With *commanet.WPF*:

1. Add Button MyCommand in XAML
2. Create Metod MyCommandExecute(...) (optionally MyCommandCanExecute(...))

Command handling methods will be auto-bound by its name.
Half of work gone. Disturbing code removed from sources.

In addition:

*MyCommandExecute(...)* is runs in separate thread in difference with WPF default command handler.
If need to do something before or/end after execution command - just add in ModelView methods:

- *MyCommandBeforeExecute(...)* - runs before command handler
- *MyCommandAfterExecute(...)* - runs after command handler
- *MyCommandException(...)* - runs if in command handler exception have been thrown


They also will be bound automatically by its name.

-----------------------------

**[DependsOf(...)] attribute***

That is boring always to define properties with stupid copy/paste NotifyPropertyChange setter... A little help was made for case when we know that if one property was changed then other properties should be notified too. In shuch case we can notify one property and all other dependent ones will be notified automatically. It made by attribution:

```C#
    public string Test
    {
        get => test;
        set{SetAndNotify(value);}
    }

    [DependsOf(nameof(Test))]
    public string Test2 {get;set;} = "Initial Value Test 2";

    [DependsOf(nameof(Test))]
    public string Test3 {get;set;} = "Initial Value Test 3";
```
As you can see above, Test2 and Test3 does not have setter implementation - it is "auto property".
Then if in code we will write:

    Test2="Something 2";
    Test3="Something 3";
    Test="Something";

Then all 3 properties will be notified at the moment when Test property changed. 
It is clear that really notified property needs to be updated last to have expected behavior.
             

