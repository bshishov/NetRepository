using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRepository
{

    /*
     * Detached: сущность не связана с контекстом и с базой
     * Added: сущность связана с контекстом, но не с базой
     * Unchanged: связана с базой и контекстом, нет изменений
     * Modified: сущность связана с базой и контекстом, имеются незакоммиченные изменения
     * Deleted: связана с базой и контекстом, помечена на удаление и при следующем коммите будет удалена     
     */


    /// <summary>
    /// Возможные состояния записи
    /// </summary>
    public enum EntryState
    {   
        /// <summary>
        /// Cущность связана с контекстом, но не с базой
        /// </summary>
        Added,

        /// <summary>
        /// Сущность связана с базой и контекстом, нет изменений
        /// </summary>
        Unchanged,

        /// <summary>
        /// Сущность связана с базой и контекстом, имеются изменения
        /// </summary>
        Modified,

        /// <summary>
        /// Связана с базой и контекстом, помечена на удаление и при следующем коммите будет удалена
        /// </summary>
        Deleted,

        /// <summary>
        /// Сущность не связана с контекстом и с базой
        /// </summary>
        Detached
    }  
}
