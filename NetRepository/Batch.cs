using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLiteTools;

namespace NetRepository
{
    public class SQLiteBatch
    {
        private bool _active;
        private SQLiteDriver _driver;

        public SQLiteBatch(SQLiteDriver driver)
        {
            _driver = driver;
        }

        public bool Active
        {
            get { return _active; }
            set { }
        }

        public void Exec(string sqlStr)
        {
            if (!_active)
            {
                throw new Exception("Tried to execute without begin transaction");
            }
            
            _driver.Query(sqlStr);
        }

        public void Begin()
        {
            if (_active)
            {
                return;
            }

            _driver.Query("PRAGMA journal_mode = OFF;");
            _driver.Query("BEGIN TRANSACTION IMMEDIATE;");
            _active = true;
        }

        public void End()
        {
            if (!_active)
            {
                throw new Exception("Batch tries to end before started");
            }

            _driver.Query("COMMIT TRANSACTION;");
            _driver.Query("PRAGMA journal_mode = MEMORY;");
            _active = false;
        }
       
    }
}
