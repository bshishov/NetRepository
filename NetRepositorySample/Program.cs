using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetRepository;
using System.IO;

namespace NetRepository
{
    public static class Logger
    {
        static System.IO.StreamWriter file;
        public static void Initialize(string path)
        {
            file = new System.IO.StreamWriter(path, true);       
        }

        public static void Log(string input, params object[] val)
        {
            Console.WriteLine(input,val);            
            file.WriteLine(input,val);            
        }

        public static void Close()
        {
            file.Close();
        }
    }

    class Timer
    {
        DateTime start;        

        public void Start()
        {
            start = DateTime.Now;
        }

        public string End(string msg = "{0}")
        {            
            TimeSpan duration = DateTime.Now - start;                        
            return String.Format(msg, duration.TotalMilliseconds);
        }
    }

    class Program
    {
        public static void SliceAdded(object sender, SliceChangedEventArgs<int,int> args)
        {
            Console.WriteLine("{1} -> {0}",args.CurrentSlice.Id, args.CurrentSlice.ParentId);
        }

        public static void DataAdded(object sender, DataChangedEventArgs<int, int> args)
        {
            Console.WriteLine("Was added {0} entries", args.Entries.Count);
        }

        public static void ObjectAdded(object sender, ObjectsChangedEventArgs<int, int> args)
        {
            Console.WriteLine("Was added {0} objects", args.Entries.Count);
        }

        public class MyClass : CachedObject<int, int>
        {
            public int Value
            {
                get { return (int)GetProperty(123).Value; }
                set { SetProperty(123, value); }
            }            
        }

        static void Main(string[] args)
        {     
            /*
            // Пример без кеширования
            // Создадим менеджер репозитория
            SQLiteRepositoryManager manager = new SQLiteRepositoryManager("path_to_my_db.db3");
            
            //Создаем начальный срез и переключаемся на него           
            Slice root = manager.AddNextSlice();
            manager.SetCurrentSlice(root);
            
            //Создаем объект, указываем тип и идентификатор
            var obj = manager.CreateObject("myType");

            //Задаем значение свойствам с идентификатором “1” и “2”
            obj.SetProperty(1,"foo");      
            obj.SetProperty(2,"bar");      

            //Создаем и переходим на следующий срез
            manager.SetCurrentSlice(manager.AddNextSlice());
            
            //Зададим свойство
            obj.SetProperty(1,"baz"); 

            //Проверим результат, Получим свойства, указанные выше. Параметры: id свойства            
            string p1 = (string)obj.GetProperty(1).Value; // baz
            string p2 = (string)obj.GetProperty(2).Value; // bar

            //Вернемся на прошлый срез и снова посмотрим результат.
            manager.SetCurrentSlice(root);
            string p3 = (string)obj.GetProperty(1).Value; // foo
            string p4 = (string)obj.GetProperty(2).Value; // bar
  
            // Закроем менеджер
            manager.Close();
            */


            // Пример с кешированием
            // Создадим менеджер репозитория
            SQLiteRepositoryManager manager = new SQLiteRepositoryManager();            

            //Создаем начальный срез и переключаемся на него           
            Slice root = manager.AddNextSlice();
            manager.SetCurrentSlice(root);

            // Создадим кеширующий репозиторий объектов MyClass (наследован от CachedObject) 
            // и назовем его "myClass Objects"
            var repository = manager.CreateRepo<MyClass>("myClass Objects");

            // Есть 2 способа создать кешированный объект:
            // 1. Вызвать метод кеширующего репозитория
            var myObject1 = repository.CreateObject();

            // 2. Создать самому и прикрепить к кеширующему репозиторию
            var myObject2 = new MyClass();
            repository.AttachObject(myObject2);

            // Зададим значения
            myObject1.SetProperty(0, "myValue1");
            myObject2.SetProperty(0, "myValue2");

            // Сохраним изменения
            repository.Commit();

            //Создаем и переходим на следующий срез
            manager.SetCurrentSlice(manager.AddNextSlice());

            //Зададим свойство
            myObject1.SetProperty(0, "myValue3");

            // Сохраним изменения
            repository.Commit();

            //Проверим результат, Получим свойства, указанные выше. Параметры: id свойства            
            string p1 = (string)myObject1.GetProperty(0).Value; // myValue3
            string p2 = (string)myObject2.GetProperty(0).Value; // myValue2

            //Вернемся на прошлый срез и снова посмотрим результат.
            manager.SetCurrentSlice(root);
            repository.Refresh();
            string p3 = (string)myObject1.GetProperty(0).Value; // myValue1
            string p4 = (string)myObject2.GetProperty(0).Value; // myValue2
        }         
    }
}

        