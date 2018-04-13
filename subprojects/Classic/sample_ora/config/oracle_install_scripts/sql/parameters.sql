--#########################################################
-- SCRIPT VARIABLES
--######################################################### 
DEFINE schemaName = SAMPLE
DEFINE schemaPass = ORACLE
DEFINE dumpFileName = 'SAMPLE_schemas.dmp'

-- Null for non cs_security applications
DEFINE privTableName = NULL
DEFINE min_priv = NULL

-- The names for the tablespaces are normally derived from the schema names but may be modified if necessary
DEFINE tableSpaceName = T_&&schemaName
DEFINE indexTableSpaceName = T_&&schemaName._INDEX
DEFINE lobsTableSpaceName = T_&&schemaName._LOBS
DEFINE tempTableSpaceName = TEMP

-- The file names are normally the tablespace name followed by DBF, but may be modified to include a path if it
-- is desired to place the files in a non-default location.

-- By default, all of these go in the same place, but seperate defines are provided for index
-- tablespaces, lob tablespaces, and table tablespaces, so that these may be easily separated.
-- Replace &&defaultIndexTSPath, &&defaultLobTSPath, or &&defaultTableTSPath with a specific
-- path in the definition of the tablespace you want to move if you want to move some of the tablespaces.

DEFINE defaultIndexTSPath= ''
DEFINE defaultLobTSPath= ''
DEFINE defaultTableTSPath= ''

DEFINE tableSpaceFile = '&&defaultTableTSPath&&tableSpaceName..DBF' 
DEFINE indexTableSpaceFile = '&&defaultIndexTSPath&&indexTableSpaceName..DBF'
DEFINE lobsTableSpaceFile = '&&defaultLobTSPath&&lobsTableSpaceName..DBF'

DEFINE dataBlockSize = 8K 
DEFINE lobBlockSize  = 8K 
DEFINE indexBlockSize = 8K

DEFINE dataBlockSizeClause = 'BLOCKSIZE &&dataBlockSize'
DEFINE lobBlockSizeClause = 'BLOCKSIZE &&lobBlockSize'
DEFINE indexBlockSizeClause = 'BLOCKSIZE &&indexBlockSize'

-- The default sizes are very small, suitable only for demo purposes.
-- Normally, these sizes should be changed to a more realistic initial size.
-- For the Table and IDX tablespaces, approximately 10M/user/year should be allocated,
-- LOB and CTX are very dependant on the number and types of documents and spectra
-- which will be stored, but 20M/user/year is a reasonable starting point.

DEFINE tablespaceSize    = 4M
DEFINE lobTablespaceSize = 4M
DEFINE idxTablespaceSize   = 4M

DEFINE tablespaceExtent   =  'AUTO'
DEFINE idxTablespaceExtent  =  'AUTO'
DEFINE lobTablespaceExtent  =  'AUTO'

-- The storage size for base64cdx data in chemical structures table
DEFINE lobB64cdx = 2K
-- The storage size for blob data in the graphics table
DEFINE lobGraphic = 4K
