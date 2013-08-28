using System;

namespace NetRepository
{
    /// <summary>
    /// Вспомогательные методы
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Формирует ключ из идентификаторов объекта и атрибута
        /// </summary>
        /// <param name="objId">Идентификатор объекта</param>
        /// <param name="attrId">Идентификатор свойства</param>
        /// <returns>Ключ</returns>
        public static string GetKey(int objId, int attrId)
        {
            return string.Format("{0}.{1}", objId, attrId);
        }

        /// <summary>
        /// Извлекает из ключа идентификаторы объекта и атрибута
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="objId">Идентификатор объекта</param>
        /// <param name="attrId">Идентификатор свойства</param>
        public static void ParseKey(string key, ref int objId, ref int attrId)
        {
            string[] values = key.Split(".,".ToCharArray());
            objId = Convert.ToInt32(values[0]);            
            attrId = values.Length == 1 ? 0 : Convert.ToInt32(values[1]);
        }
    }
    
    public static class Constants
    {
        /// <summary>
        /// Имя файла базы данных по-умолчанию
        /// </summary>
        public const string DataBaseName = "repository.db3";

        /// <summary>
        /// Имя файла со скриптом иницализации базы
        /// </summary>
        public const string InitialSql = "Initial.sql";

        /// <summary>
        /// Тип объекта по умолчанию
        /// </summary>
        public const string DefaultObjType = "default";
    }

    /// <summary>
    /// Строковые SQL константы для выборок и вставок в репозиторий
    /// </summary>
    public static class SQLConstants
    {             
        /// <summary>
        /// Выборка ключа и значения с указанным ключом и срезом
        /// </summary>
        public const string SelectPropSql = @"            
            select key, value from Data d                        
            left join ParentSlices ps     
            on d.slice_id = ps.parent_slice_id
            where d.key = {1} and ps.child_slice_id = {0}
            order by d.slice_id desc            
        ";

        /// <summary>
        /// Выборка геоданных
        /// </summary>
        public const string SelectGeoDataSql = @"            
            select geom from GeoData d                        
            left join ParentSlices ps     
            on d.slice_id = ps.parent_slice_id
            where d.object_id = {1} and ps.child_slice_id = {0}
            order by d.slice_id desc            
        ";

        /// <summary>
        /// Выборка значений и ключей для всех атрибутов объекта
        /// </summary>
        public const string SelectObjDataSql = @"
            select key, value from Data d                        
            left join ParentSlices ps     
            on d.slice_id = ps.parent_slice_id
            where d.object_id = {1} and ps.child_slice_id = {0}
            group by d.key;
        ";

        /// <summary>
        /// Выборка ключа и значения с возможностью фильтрации
        /// </summary>
        public const string SelectCustomStatement = @"
            select * from (
            select key, value, object_id from Data d                        
            left join ParentSlices ps     
            on d.slice_id = ps.parent_slice_id
            where ps.child_slice_id = {0}
            group by d.key )
            where {1}
        ";

        /// <summary>
        /// Выборка всех объектов с обределенным типом
        /// </summary>
        public const string SelectObjectsByType = "SELECT id FROM Objects WHERE type = '{0}'";

        /// <summary>
        /// Выборка объекта как такового
        /// </summary>
        public const string SelectObject = "SELECT id, type FROM Objects WHERE object_id = {0}";
   
        /// <summary>
        /// Выборка всех данных у которых срез равен заданному
        /// </summary>
        public const string SelectSlice = "SELECT * FROM Data WHERE slice_id = {0}";

        /// <summary>
        /// Выборка всех доступных срезов (По возрастанию)
        /// </summary>
        public const string SelectSlicesAsc = "SELECT * FROM Slices ORDER BY id ASC";

        /// <summary>
        /// Выборка последнего среза (срез с последним идентификатором)
        /// </summary>
        public const string SelectLastSlice = "SELECT id, parent_slice_id FROM Slices ORDER BY id DESC;";
        
        /// <summary>
        /// Выборка идентификатора последнего объекта
        /// </summary>
        public const string SelectLastObjectId = "SELECT id FROM Objects ORDER by id DESC";
        
        
        /// <summary>
        /// Скрипт полной очистки таблицы
        /// </summary>
        public const string WipeSql = @"
            PRAGMA writable_schema = 1;
            delete from sqlite_master where type in ('table', 'index', 'trigger');
            PRAGMA writable_schema = 0;
        ";

        /// <summary>
        /// Скрипт очистки таблицы от данных
        /// </summary>
        public const string TruncateSql = @"
            delete from Objects;
            delete from ParentSlices;  
            delete from SlicesData;
            delete from Slices;            
            delete from Data;
        ";

        /// <summary>
        /// Скрипт сжатия таблицы
        /// </summary>
        public const string VacuumSql = "VACUUM;";

        /// <summary>
        /// Запрос на вставка данных (параметры: 1 - значение, 2 - идентификатор среза, 3 - ключ, 4 - идентификатор объекта)
        /// </summary>
        public const string InsertData = "INSERT INTO BufferData (value, slice_id, key, object_id) VALUES ('{0}',{1},'{2}',{3});";

        /// <summary>
        /// Запрос на вставку гео данных (параметры: 1 - объект геометрии, 2 - идентификатор среза, 3 - идентификатор объекта)
        /// </summary>
        public const string InsertGeoData = "INSERT INTO GeoData (geom, slice_id, object_id) VALUES ('{0}',{1},{2});";

        /// <summary>
        /// Запрос на вставку среза (параметры: 1 - идентификатор, 2 - идентификатор родительского среза)
        /// </summary>
        public const string InsertSlice = "INSERT INTO Slices (id,parent_slice_id) VALUES (NULL,{0});";

        /// <summary>
        /// Запрос на вставку объекта (параметры: 1 - идентификатор)
        /// </summary>
        public const string InsertObject = "INSERT INTO Objects (id, type) VALUES ('{0}', '{1}');";
        
        /// <summary>
        /// Запрос на перенос содержимого буферной таблицы в таблицу с данными
        /// </summary>
        public const string FromBuffer = "INSERT INTO Data SELECT * FROM BufferData;";
        
        /// <summary>
        /// Запрос на очищение буферной таблицы
        /// </summary>
        public const string WipeBuffer = "DELETE FROM BufferData;";
    }
}
