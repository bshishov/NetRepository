-- Table: Values
CREATE TABLE Data  ( 	
	slice_id        INTEGER  REFERENCES Slices ( id ),
    key             REAL,    	
    object_id       INTEGER REFERENCES Objects ( id ),        
	value           NONE
);

-- Table: BufferValues
CREATE TABLE BufferData  (    
    slice_id        INTEGER  REFERENCES Slices ( id ),
    key             REAL,    
    object_id       INTEGER REFERENCES Objects ( id ),        
    value           NONE
);

-- Table: Slices
CREATE TABLE Slices  ( 
    id  INTEGER PRIMARY KEY AUTOINCREMENT,
    parent_slice_id INTEGER     REFERENCES Slices ( id )
);


-- Table: Parent Slices
CREATE TABLE ParentSlices  (
    child_slice_id INTEGER      REFERENCES Slices ( id ),
    parent_slice_id INTEGER     REFERENCES Slices ( id )
);

-- Table: SlicesData
CREATE TABLE SlicesData  ( 
    id              INTEGER PRIMARY KEY,
    slice_id        INTEGER     REFERENCES Slices ( id ),
    property_name   VARCHAR( 1000 ),
    value           NONE
);

-- Table: Objects
CREATE TABLE Objects ( 
    id      INTEGER PRIMARY KEY,
    type    VARCHAR (1000)    
);

-- Indexes:
CREATE INDEX DataIndex1 ON Data ( slice_id );
CREATE INDEX DataIndex2 ON Data ( key, slice_id );
CREATE INDEX DataIndex3 ON Data ( object_id, slice_id );

-- Bad Performance indexes
-- CREATE INDEX DataIndex4 ON Data ( key );
-- CREATE INDEX DataIndex5 ON Data ( object_id );
-- CREATE INDEX DataIndex6 ON Data ( slice_id, key );
-- CREATE INDEX DataIndex7 ON Data ( slice_id, object_id );

CREATE INDEX ParentSlices1 ON ParentSlices ( child_slice_id );
CREATE INDEX ParentSlices2 ON ParentSlices ( parent_slice_id );
CREATE INDEX ParentSlices3 ON ParentSlices ( child_slice_id, parent_slice_id  );

CREATE INDEX ObjectIndex1 ON Objects ( id );

CREATE INDEX SliceIndex1 ON Slices ( id );

-- Triggers:
CREATE TRIGGER SliceAdd 
    AFTER insert ON Slices
    BEGIN
        INSERT INTO ParentSlices (child_slice_id, parent_slice_id) 
            SELECT new.id, parent_slice_id FROM ParentSlices 
                WHERE child_slice_id = new.parent_slice_id;
        INSERT INTO ParentSlices (child_slice_id, parent_slice_id) 
            VALUES (new.id, new.id);
    END;

-- Initial configuration:
PRAGMA recursive_triggers = TRUE;
PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;
PRAGMA temp_store = MEMORY;