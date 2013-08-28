using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using SQLiteTools;

namespace NetRepository
{
    /// <summary>
    /// Аргументы события изменения данных
    /// </summary>
    public class DataChangedEventArgs<TObjKey, TPropKey> : EventArgs
    {
        public EntryState State { get; private set; }
        public List<Property<TObjKey, TPropKey>> Entries { get; private set; }

        public DataChangedEventArgs()
        {
            Entries = new List<Property<TObjKey, TPropKey>>();
        }
        public DataChangedEventArgs(EntryState state, Property<TObjKey, TPropKey> data)
        {
            State = state;
            Entries = new List<Property<TObjKey, TPropKey>>();
            Entries.Add(data);
        }
        public DataChangedEventArgs(EntryState state, List<Property<TObjKey, TPropKey>> data)
        {
            State = state;
            Entries = data;
        }
        public void Add(EntryState state, Property<TObjKey, TPropKey> data)
        {           
            State = state;
            Entries.Add(data);
        }
    }

    /// <summary>
    /// Аргументы события изменения среза
    /// </summary>
    public class SliceChangedEventArgs<TObjKey, TPropKey> : EventArgs
    {
        public Slice LastSlice { get; private set; }
        public Slice CurrentSlice { get; private set; }

        public SliceChangedEventArgs(Slice last, Slice current)
        {
            LastSlice = last;
            CurrentSlice = current;
        }
    }

    /// <summary>
    /// Аргументы события изменения объектов
    /// </summary>
    public class ObjectsChangedEventArgs<TObjKey, TPropKey> : EventArgs
    {
        public EntryState State { get; private set; }
        public List<ObjectEntry<TObjKey, TPropKey>> Entries { get; private set; }

        public ObjectsChangedEventArgs()
        {
            Entries = new List<ObjectEntry<TObjKey, TPropKey>>();
        }
        public ObjectsChangedEventArgs(EntryState state, ObjectEntry<TObjKey, TPropKey> obj)
        {
            State = state;
            Entries = new List<ObjectEntry<TObjKey, TPropKey>>();
            Entries.Add(obj);
        }
        public ObjectsChangedEventArgs(EntryState state, List<ObjectEntry<TObjKey, TPropKey>> objs)
        {
            State = state;
            Entries = objs;
        }
        public void Add(EntryState state, ObjectEntry<TObjKey, TPropKey> data)
        {
            State = state;
            Entries.Add(data);
        }
    }

    /// <summary>
    /// Репозиторий на основе SQLite, Для оптимизации использует int ключи для 
    /// идентификации объектов и атрибутов
    /// </summary>
    public class SQLiteRepositoryManager : IBaseRepository<int, int>
    {
        #region Поля

        private SQLiteDriver _driver;
        private SQLiteBatch _batch;
        private string _dbPath;
        private Slice _currentSlice;        
        private Dictionary<string, IEnumerable<CachedObject<int, int>>> _repositories;
        private int _lastObjectKey = 0;
        private int _activeBatches = 0;
        private bool _isBufferEmpty = true;

        #endregion
        
        #region Конструкторы
        public SQLiteRepositoryManager(string dbPath = Constants.DataBaseName)
        {
            _dbPath = dbPath;
            _repositories = new Dictionary<string, IEnumerable<CachedObject<int, int>>>();
            Init();           


            SQLiteDataReader reader = _driver.Query(SQLConstants.SelectLastObjectId);
            if (reader.HasRows)
            {
                reader.Read();
                _lastObjectKey = reader.GetInt32(0);
            }
            else
            {
                _lastObjectKey = 0;
            }            
        }
        #endregion

        #region Свойства
        public Slice CurrentSlice
        {
            get {
                if (_currentSlice == null)
                    _currentSlice = new Slice(-1);
                return _currentSlice; 
            }
        }
        #endregion

        #region Методы интерфейса IBaseRepository
        /// <summary>
        /// Связывает свойство с менеджером, устанавливает состояние "Added" если было "Detached"
        /// </summary>
        /// <param name="prop"></param>
        public void Attach(Property<int, int> prop)
        {
            prop.SetManager(this);

            if(prop.State == EntryState.Detached)
                prop.SetState(EntryState.Added);            
        }
        /// <summary>
        /// Связывает объект с менеджером, устанавливает состояние "Added" если было "Detached"
        /// </summary>
        /// <param name="prop"></param>
        public void Attach(ObjectEntry<int, int> obj)
        {
            obj.SetManager(this);            

            if (obj.State == EntryState.Detached)
                obj.SetState(EntryState.Added);
        }

        public int GetUniqueObjKey()
        {
            return ++_lastObjectKey;
        }

        public Slice AddSlice(Slice parent)
        {            
            int parentId = parent.Id;
            if (parentId < 0 && _currentSlice != null)
            {
                parentId = _currentSlice.ParentId;
            }

            StartBatch();
            _batch.Exec(String.Format(SQLConstants.InsertSlice, parentId < 0 ? "NULL" : parentId.ToString()));
            EndBatch();

            SQLiteDataReader reader = _driver.Query(SQLConstants.SelectLastSlice);
            if (reader.HasRows)
            {
                reader.Read();
                int sliceId = reader.GetInt32(0);                

                if (parentId < 0)
                    return new Slice(sliceId);
                return new Slice(sliceId, parentId);
            }

            return null;
        }
        public Slice AddNextSlice()
        {   
            return AddSlice(CurrentSlice);
        }
        public void SetCurrentSlice(Slice slice, bool autoCommit = false)
        {
            Slice last = _currentSlice;
            _currentSlice = slice;

            OnSliceChange(new SliceChangedEventArgs<int, int>(last, _currentSlice));
            return;
        }
        public void NextSlice()
        {
            SetCurrentSlice(AddNextSlice(), true);
        }
        public void RemoveSlice(Slice slice)
        {
            // SQL REMOVE
        }
        
        public Property<int, int> CreateProperty(int objKey, int attrKey, object value = null)
        {
            var property = new Property<int, int>(objKey, attrKey, value);            
            Attach(property);
            return property;
        }
        public Property<int, int> GetProperty(int objKey, int attrKey)
        {
            return GetProperty(Utilities.GetKey(objKey, attrKey), _currentSlice.Id);                        
        }
        public void SetProperty(Property<int, int> data) 
        {
            SetValue(data.ObjectId, data.Id, data.Value);
            data.SetState(EntryState.Unchanged);
        }
        public T GetValue<T>(int objKey, int attrKey)
        {
            return (T)GetProperty(objKey, attrKey).Value;
        }
        public void SetValue(int objKey, int attrKey, object value)
        {
            StartBatch();
            _batch.Exec(String.Format(SQLConstants.InsertData,
                value,
                _currentSlice.Id,
                Utilities.GetKey(objKey, attrKey),
                objKey
                ));
            _isBufferEmpty = false;
            EndBatch();
        }
        
        public void FetchData(Property<int, int> data)
        {
            // SQL select and save changes to "data"
        }
        public void RemoveProperty(int objKey, int attrKey)
        {
            // SQL remove
        }
        public List<Property<int, int>> GetObjectProperties(int objKey)
        {
           var results = new List<Property<int, int>>();

            SQLiteDataReader reader = _driver.Query(
                String.Format(SQLConstants.SelectObjDataSql,_currentSlice.Id,objKey)
                );
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string key = reader.GetValue(0).ToString();
                    int objId = 0;
                    int attrId = 0;
                    Utilities.ParseKey(key, ref objId, ref attrId);
                    var property = new Property<int, int>(objId,attrId,reader.GetValue(1));
                    property.SetState(EntryState.Unchanged);
                    Attach(property);
                    results.Add(property);
                }
                return results;
            }

            return null;
        }

        public ObjectEntry<int, int> CreateObject(int objKey, string type)
        {
            StartBatch();
            _batch.Exec(String.Format(SQLConstants.InsertObject, objKey, type));
            EndBatch();        
            
            // Сохраняем целостность ключей
            _lastObjectKey = Math.Max(_lastObjectKey, objKey);

            var obj = new ObjectEntry<int, int>(objKey, type);
            Attach(obj);
            obj.SetState(EntryState.Unchanged);
            return obj;            
        }
        public ObjectEntry<int, int> CreateObject(string type)
        {
            return CreateObject(GetUniqueObjKey(), type);
        }
        public ObjectEntry<int, int> GetObject(int objKey)
        {
            SQLiteDataReader reader = _driver.Query(
                String.Format(
                    SQLConstants.SelectObject,
                    objKey
                )
                );
            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    var obj = new ObjectEntry<int, int>(reader.GetInt32(0), reader.GetString(1));
                    Attach(obj);
                    obj.SetState(EntryState.Unchanged);
                    return obj;
                }
            }
            return null;
        }
        public List<T> GetObjectsByType<T>(string type) where T : ObjectEntry<int, int>
        {
            var results = new List<T>();

            SQLiteDataReader reader = _driver.Query(
                String.Format(SQLConstants.SelectObjectsByType,type)
                );
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var obj = Activator.CreateInstance<T>();
                    Attach(obj);
                    obj.SetType(type);
                    obj.SetId(reader.GetInt32(0));
                    obj.SetState(EntryState.Unchanged);                    
                    results.Add((T)obj);
                }
                return results;
            }

            return null;
        }
        public void RemoveObject(int objKey)
        {
            // SQL DELETE
        }        
        
        public CacheRepository<T, int, int> CreateRepo<T>(string name) where T : CachedObject<int, int>
        {
            if (!_repositories.ContainsKey(name))
            {
                CacheRepository<T, int, int> repo = new CacheRepository<T, int, int>(this, name);
                _repositories.Add(name, repo);
                return repo;
            }
            else
            {
                throw new ArgumentException("Repository already created");
            }
        }
        public CacheRepository<T, int, int> GetRepo<T>(string name) where T : CachedObject<int, int>
        {
            if (_repositories.ContainsKey(name))
            {
                return (CacheRepository<T, int, int>)_repositories[name];
            }
            else
            {
                throw new ArgumentException("No repository with specified key");
            }
        }
        public void RemoveRepo(string name)
        {
            if(_repositories.ContainsKey(name))
            {
                _repositories.Remove(name);
            }
            else
            {
                throw new ArgumentException("No repository with specified key");
            }
        }

        public bool HasChanges()
        {            
            foreach (CacheRepository<CachedObject<int,int>, int, int> repo in _repositories.Values)
            {
                if (repo.HasChanges())
                {
                    return true;
                }
            }
            return false;
        }
        public void Commit()
        {
            _batch.Begin();
            foreach (var repo in _repositories.Values)
            {
                foreach (var obj in repo)
                {
                    obj.Save();
                }
            }
            _batch.End();
        }
        #endregion       
        
        #region Методы
        private void Init()
        {
            bool initial = false;
            if (!File.Exists(_dbPath))
                initial = true;            

            _driver = new SQLiteDriver(_dbPath);

            if (_dbPath == ":memory:")
            {
                Wipe();
                initial = true;
            }

            if (initial)
            {
                StreamReader reader = new StreamReader(Constants.InitialSql);

                try
                {
                    _driver.Query(reader.ReadToEnd());
                    reader.Close();
                }
                catch (System.Exception ex)
                {
                    reader.Close();
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Truncate();
                Vacuum();
            }
            
            _batch = new SQLiteBatch(_driver);
            _currentSlice = new Slice(-1);
        }
                
        public List<Property<int,int>> GetSlice(Slice slice)
        {
            SQLiteDataReader reader = _driver.Query(string.Format(SQLConstants.SelectSlice, slice.Id));
            List<Property<int, int>> results = new List<Property<int, int>>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    /// SQL NOT WORKING
                    Property<int, int> property = new Property<int, int>(                    
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetValue(2)
                    );
                    results.Add(property);
                    property.SetState(EntryState.Added);
                    Attach(property);
                }
                return results;
            }

            throw new Exception("Element Not Found");
        }
        public List<Slice> GetSlices()
        {
            SQLiteDataReader reader = _driver.Query(SQLConstants.SelectSlicesAsc);
            List<Slice> results = new List<Slice>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    results.Add(new Slice(reader.GetInt32(0), reader.GetInt32(1)));
                }
                return results;
            }

            throw new Exception("Element Not Found");
        }

        public Property<int, int> GetProperty(string key, int sliceId = -1)
        {
            SQLiteDataReader reader = _driver.Query(
                String.Format(
                    SQLConstants.SelectPropSql,
                    sliceId < 0 ? _currentSlice.Id : sliceId,
                    key
                )
                );
            if (reader.HasRows)
            {
                reader.Read();
                int objId = 0;
                int attrId = 0;
                Utilities.ParseKey(key, ref objId, ref attrId);
                var property = new Property<int, int>(objId,attrId,reader.GetValue(1));
                Attach(property);
                property.SetState(EntryState.Unchanged);
                return property;
            }

            return null;           
        }        
        
        public void StartBatch()
        {
            if (_activeBatches == 0)
                _batch.Begin();

            _activeBatches += 1;
        }
        public void EndBatch()
        {
            if (_activeBatches == 1)
            {
                _batch.End();

                if(!_isBufferEmpty)
                    FromBuffer();
            }

            _activeBatches -= 1;
        }        

        public void Wipe()
        {
            _driver.Query(SQLConstants.WipeSql);
        }
        public void Truncate()
        {
            _driver.Query(SQLConstants.TruncateSql);
        }
        public void Vacuum()
        {
            _driver.Query(SQLConstants.VacuumSql);
        }
        public void Close()
        {
            _driver.Close();
        }
        public void FromBuffer()
        {
            _driver.Query(SQLConstants.FromBuffer);
            _driver.Query(SQLConstants.WipeBuffer);
            _isBufferEmpty = true;
        }
        #endregion

        #region События
        public event EventHandler<DataChangedEventArgs<int,int>> DataChanged;
        public event EventHandler<ObjectsChangedEventArgs<int, int>> ObjectsChanged;
        public event EventHandler<SliceChangedEventArgs<int, int>> SliceChanged;

        protected DataChangedEventArgs<int, int> DataEventArgsBuffer;
        protected ObjectsChangedEventArgs<int, int> ObjectsEventArgsBuffer;

        protected void OnSliceChange(SliceChangedEventArgs<int, int> e)
        {
            if (SliceChanged != null)
            {
                SliceChanged(this, e);
            }
        }
        protected void OnObjectsChange(ObjectsChangedEventArgs<int, int> e)
        {
            if (ObjectsChanged != null)
            {
                ObjectsChanged(this, e);
            }
        }
        protected void OnDataChange(DataChangedEventArgs<int, int> e)
        {
            if (DataChanged != null)
            {
                DataChanged(this, e);
            }
        }
        #endregion 
    }
}