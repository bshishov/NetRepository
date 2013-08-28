using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRepository
{
    /// <summary>
    /// Интерфейс кеширующего репозитория
    /// </summary>
    /// <typeparam name="T">Тип объектов хранящихся в рпозитории (наследованы от RepoEntity)</typeparam>
    /// <typeparam name="TObjKey">Тип идентификаторов объектов</typeparam>
    public interface ICachingRepository<T, TObjKey, TPropKey> : IEnumerable<T>
       where T : CachedObject<TObjKey, TPropKey>
    {
        void AttachObject(T obj);
        T GetObject(TObjKey objKey);
        T CreateObject(TObjKey objKey);
        T CreateObject();
        void RemoveObject(TObjKey objKey);        
        bool HasChanges();
        void Refresh();
        void Commit();
    }
}
