using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRepository
{
    /// <summary>
    /// Базовый класс для всех сущностей репозитория
    /// </summary>
    public abstract class RepoEntity<TObjKey, TPropKey>
    {
        #region Поля
        protected int _sliceId;
        protected EntryState _state;
        protected IBaseRepository<TObjKey, TPropKey> _manager; 
        #endregion        
        
        #region Свойства
        public int SliceId { get { return _sliceId; } }
        public IBaseRepository<TObjKey, TPropKey> Manager { get { return _manager; } }
        public EntryState State { get { return _state; } }        
        #endregion      
        
        #region Конструкторы
        public RepoEntity()
        {
            _state = EntryState.Detached;
        } 
        #endregion

        #region Методы
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Удалить данные из БД
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Данные актуальны или нет
        /// </summary>
        /// <returns></returns>
        public bool IsRelevant()
        {
            if (Manager != null)
            {
                return Manager.CurrentSlice.Id == _sliceId;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Устанавливает состояние объекта
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(EntryState state)
        {
            _state = state;
        }
        
        /// <summary>
        /// Устанавливает срез в которм данные были созданы или измененены 
        /// </summary>
        /// <param name="sliceId"></param>
        internal void SetSLiceId(int sliceId)
        {
            _sliceId = sliceId;
        }

        /// <summary>
        /// Устанавливает менеджер
        /// </summary>
        /// <param name="manager"></param>
        internal void SetManager(IBaseRepository<TObjKey, TPropKey> manager)
        {
            _manager = manager;
            _sliceId = manager.CurrentSlice.Id;
        } 
        #endregion
    }
}
