using System.Collections.Generic;

namespace NetRepository
{
    /// <summary>
    /// Базовый репозиторий, работает не кешируя
    /// </summary>
    /// <typeparam name="TObjKey">Тип идентификаторов объектов</typeparam>
    /// <typeparam name="TPropKey">Тип идентификаторов атрибутов</typeparam>
    public interface IBaseRepository<TObjKey,TPropKey>
    {
        Slice CurrentSlice { get; }

        Slice AddSlice(Slice parent);
        Slice AddNextSlice();
        void SetCurrentSlice(Slice slice, bool autoCommit = false);
        void NextSlice();
        void RemoveSlice(Slice slice);        

        void Attach(Property<TObjKey, TPropKey> prop);
        void Attach(ObjectEntry<TObjKey, TPropKey> obj);

        TObjKey GetUniqueObjKey();

        Property<TObjKey, TPropKey> CreateProperty(TObjKey objKey, TPropKey attrKey, object value = null);
        Property<TObjKey, TPropKey> GetProperty(TObjKey objKey, TPropKey attrKey);
        void SetProperty(Property<TObjKey, TPropKey> data);        
        T GetValue<T>(TObjKey objKey, TPropKey attrKey);
        void SetValue(TObjKey objKey, TPropKey attrKey, object value);        
        void FetchData(Property<TObjKey, TPropKey> data);
        void RemoveProperty(TObjKey objKey, TPropKey attrKey);
        List<Property<TObjKey, TPropKey>> GetObjectProperties(TObjKey objKey);

        ObjectEntry<TObjKey,TPropKey> CreateObject(TObjKey objKey, string type);
        ObjectEntry<TObjKey, TPropKey> CreateObject(string type);
        ObjectEntry<TObjKey, TPropKey> GetObject(TObjKey objKey);

        List<T> GetObjectsByType<T>(string type) where T : ObjectEntry<TObjKey, TPropKey>;
        void RemoveObject(TObjKey objKey);
        
        CacheRepository<T, TObjKey, TPropKey> CreateRepo<T>(string name) where T : CachedObject<TObjKey, TPropKey>;
        CacheRepository<T, TObjKey, TPropKey> GetRepo<T>(string name) where T : CachedObject<TObjKey, TPropKey>;
        void RemoveRepo(string name);

        void StartBatch();
        void EndBatch();

        bool HasChanges();
        void Commit();        
    }  
}
