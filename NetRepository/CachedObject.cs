using System;
using System.Collections.Generic;
using System.Linq;

namespace NetRepository
{
    /// <summary>
    /// Объект репозитория с внутренним хранилищем свойств
    /// </summary>
    /// <typeparam name="TObjKey">Тип идентификатора объекта</typeparam>
    /// <typeparam name="TPropKey">Тип идентификатора свойства</typeparam>
    public class CachedObject<TObjKey, TPropKey> : ObjectEntry<TObjKey, TPropKey>
    {
        #region Конструкторы
        public CachedObject()
        {
            Data = new List<Property<TObjKey, TPropKey>>();
        }

        public CachedObject(TObjKey id, string type = Constants.DefaultObjType)
            : base(id, type)
        {
            Data = new List<Property<TObjKey, TPropKey>>();
        } 
        #endregion

        #region Свойства

        /// <summary>
        /// Список свойств объекта
        /// </summary>
        protected List<Property<TObjKey, TPropKey>> Data
        {
            get;
            set;
        } 

        #endregion

        #region Методы
        
        /// <summary>
        /// Устанавливает значение свойства объекта (во внутреннем хранилище)
        /// </summary>
        /// <param name="propertyId">Идентификатор свойства</param>
        /// <param name="value">Значение</param>
        public override void SetProperty(TPropKey propertyId, object value)
        {
            var data = Data.FirstOrDefault((d) => d.Id.Equals(propertyId));
            if (data != null)
            {
                if (!data.IsRelevant())
                    data.Value = value;
                else
                    throw new ArgumentException("Property allready exist");
            }
            else
            {
                var property = new Property<TObjKey, TPropKey>(Id, propertyId, value);
                Manager.Attach(property);
                property.SetState(EntryState.Modified);                
                Data.Add(property);
            }
        }

        /// <summary>
        /// Получает значение свойства объекта из внутреннего хранилища объекта
        /// </summary>
        /// <param name="propertyId">Идентификатор свойства</param>
        /// <returns></returns>
        public override Property<TObjKey, TPropKey> GetProperty(TPropKey propertyId)
        {
            var data = Data.FirstOrDefault((d) => d.Id.Equals(propertyId));
            if (data != null)
            {
                return data;
            }
            else
            {
                throw new ArgumentException("No such key");
            }
        }

        /// <summary>
        /// Загружает актуальные данные из репозитория во внутреннее хранилище объекта
        /// </summary>
        public void Refresh()
        {
            Data = base.GetProperties();
            SetState(EntryState.Unchanged);
        }

        /// <summary>
        /// Сохранение кэшированных данных в репозиторий
        /// </summary>
        public override void Save()
        {
            if (State == EntryState.Detached || State == EntryState.Added)
                base.Save();

            _state = EntryState.Unchanged;

            foreach (var property in Data)
            {
                property.Save();
            }
        } 
        #endregion
    }
}
