using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace NetRepository
{
    /// <summary>
    /// Кеширующий слой репозитория
    /// </summary>
    /// <typeparam name="T">Тип объектов хранящихся в репозитории (наследованы от RepoEntity)</typeparam>
    /// <typeparam name="TObjKey">Тип идентификаторов объектов</typeparam>
    /// <typeparam name="TPropKey">Тип идентификаторов атрибутов</typeparam>
    public class CacheRepository<T, TObjKey, TPropKey> : ICachingRepository<T, TObjKey, TPropKey>
        where T : CachedObject<TObjKey, TPropKey>
    {
        #region Поля
        protected string _name;
        protected List<T> _objects;
        protected IBaseRepository<TObjKey, TPropKey> _manager; 
        #endregion

        #region Свойства
        public string Name { get { return _name; } }
        public IBaseRepository<TObjKey, TPropKey> Manager { get { return _manager; } } 
        #endregion

        #region Конструкторы
        public CacheRepository(IBaseRepository<TObjKey, TPropKey> baseRepository, string name)
        {
            _name = name;
            _objects = new List<T>();
            _manager = baseRepository;

            Refresh();
        }        
        #endregion

        #region Методы
        public void AttachObject(T obj)
        {
            obj.SetType(_name);
            _manager.Attach(obj);
            _objects.Add(obj);
        }

        public T GetObject(TObjKey objKey)
        {
            var o = _objects.FirstOrDefault((obj) => obj.Id.Equals(objKey));
            if (o != null)
            {
                return o;                
            }
            else
            {                
                throw new ArgumentException("No such object in repository");                
            }
        }

        public T CreateObject(TObjKey objKey)
        {
            var o = _objects.FirstOrDefault((obj) => obj.Id.Equals(objKey));
            if (o == null)
            {
                var newObj = Activator.CreateInstance<T>();
                newObj.SetId(objKey);
                AttachObject((T)newObj);                                
                return (T)newObj;
            }
            else
            {
                throw new ArgumentException("Object is already in repository, use Get");
            }
        }

        public T CreateObject()
        {
            return CreateObject(Manager.GetUniqueObjKey());
        }

        public void RemoveObject(TObjKey objKey)
        {
            var o = _objects.FirstOrDefault((obj) => obj.Id.Equals(objKey));
            if (o != null)
            {
                _objects.Remove(o);
            }
            else
            {
                throw new ArgumentException("No such object in repository");
            }
        }

        public bool HasChanges()
        {            
            foreach (T obj in _objects)
            {
                if (obj.State == EntryState.Modified || obj.State == EntryState.Deleted)
                {
                    return true;
                }
            }

            return false;
        }

        public void Refresh()
        {
            // Пометим все как неактуальные
            foreach (var obj in _objects)
                obj.SetState(EntryState.Detached);

            // Запрос актуальных объектов
            var objects = _manager.GetObjectsByType<T>(_name);
            
            if (objects == null)
            {
                _objects.Clear();
                return;
            }

            foreach (var obj in objects)            
            {
                // ищем объекты с таким ключом                 
                var o = _objects.FirstOrDefault(p => obj.Id.Equals(p.Id));
                
                // Если объекта с таким ключом еще нет
                if (o == null)
                {
                    _objects.Add((T)obj);
                    obj.Refresh();
                }
                else
                {
                    o.Refresh();
                }
            }
            
            // Удаляем не обновленные
            _objects.RemoveAll(p => p.State == EntryState.Detached);            
        }

        public void Commit()
        {
            Manager.StartBatch();
            foreach (var obj in _objects)
            {                
                obj.Save();    
            }            
            Manager.EndBatch();
        }
        #endregion

        #region Методы IEnumerable
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return _objects.GetEnumerator();            
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _objects.GetEnumerator();
        }
        #endregion        
    }   
}
