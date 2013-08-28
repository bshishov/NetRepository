using System;
using System.Collections.Generic;

namespace NetRepository
{
    /// <summary>
    /// Объект репозиторя
    /// </summary>
    /// <typeparam name="TObjKey">Тип идентификатора объекта</typeparam>
    /// <typeparam name="TPropKey">Тип идентификатора свойства</typeparam>
    public class ObjectEntry<TObjKey, TPropKey> : RepoEntity<TObjKey, TPropKey>
    {
        #region Поля

        TObjKey _id;
        string _type; 

        #endregion

        #region Свойства

        public TObjKey Id { get { return _id; } }
        public string Type { get { return _type; } }       

        #endregion  

        #region Конструкторы
        public ObjectEntry()
        {            
        }

        public ObjectEntry(TObjKey id, string type)
        {
            _id = id;
            _type = type;
        }        
        #endregion

        #region Методы
        /// <summary>
        /// Генерирует уникальный ключ для свойства (в пределах объекта)
        /// </summary>
        /// <returns></returns>
        public TPropKey GetUniquePropKey()
        {
            return default(TPropKey);
        }

        /// <summary>
        /// Получает конкретную запись значения этого объекта
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns>Значение</returns>
        public virtual Property<TObjKey, TPropKey> GetProperty(TPropKey propertyId)
        {
            return Manager.GetProperty(_id, propertyId);
        }

        /// <summary>
        /// Устанавливает значение свойства объекта
        /// </summary>
        /// <param name="propertyId">Идентификатор свойства</param>
        /// <param name="value">Значение</param>
        public virtual void SetProperty(TPropKey propertyId, object value)
        {
            Manager.SetValue(_id, propertyId, value);
        }

        /// <summary>
        /// Получает все записи значений в текущем срезе данных для этого объекта
        /// </summary>
        /// <returns>Список значений</returns>
        public List<Property<TObjKey, TPropKey>> GetProperties()
        {
            return Manager.GetObjectProperties(_id);
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Сохранение объекта. Если объект не присоединен к базе, он присоединяется
        /// </summary>
        public override void Save()
        {
            // Если объект не в БД
            if (State == EntryState.Detached || State == EntryState.Added)
            {
                var obj = Manager.CreateObject(_id, _type);
                _id = obj.Id;
                _type = obj.Type;
                obj.SetState(EntryState.Unchanged);                
            }
        } 

        /// <summary>
        /// Устанавливает тип объекта
        /// </summary>
        /// <param name="type"></param>
        internal void SetType(string type) 
        {
            _type = type;
        }

        /// <summary>
        /// Устанавливает идентификатор объекта
        /// </summary>
        /// <param name="id"></param>        
        internal void SetId(TObjKey id)
        {
            _id = id;
        }

        #endregion
    }    
}
