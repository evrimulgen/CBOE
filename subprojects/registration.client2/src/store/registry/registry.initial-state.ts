import {
  IRecords, IRecordsRecord,
  IRegistry, IRegistryRecord,
} from './registry.types';
import { makeTypedFactory, TypedRecord } from 'typed-immutable-record';

const INITIAL_RECORDS = makeTypedFactory<IRecords, IRecordsRecord>(
  {
    temporary: false,
    rows: [],
    gridColumns: [{
      dataField: 'ID',
      dataType: 'number',
      visible: false,
    }, {
      dataField: 'NAME',
      dataType: 'string',
      caption: 'Name',
    }, {
      dataField: 'CREATED',
      dataType: 'date',
      caption: 'Created',
    }, {
      dataField: 'MODIFIED',
      dataType: 'date',
      caption: 'Modified',
    }, {
      dataField: 'CREATOR',
      caption: 'Created By',
      lookup: {},
    }, {
      dataField: 'STRUCTURE',
      dataType: 'string',
      allowFiltering: false,
      cellTemplate: 'cellTemplate',
      caption: 'Structure',
      width: 160,
    }, {
      dataField: 'REGNUMBER',
      dataType: 'string',
      caption: 'Reg Number',
    }, {
      dataField: 'STATUS',
      dataType: 'number',
      caption: 'Status',
    }, {
      dataField: 'APPROVED',
      dataType: 'string',
      caption: 'Approved',
    }]
  }
)();

const INITIAL_TEMP_RECORDS = makeTypedFactory<IRecords, IRecordsRecord>(
  {
    temporary: true,
    rows: [],
    gridColumns: [{
      dataField: 'ID',
      dataType: 'number',
      visible: false,
    }, {
      dataField: 'BATCHID',
      dataType: 'number',
      caption: 'Batch ID',
    }, {
      dataField: 'MW',
      dataType: 'number',
      caption: 'MW',
    }, {
      dataField: 'MF',
      dataType: 'string',
      caption: 'MF',
    }, {
      dataField: 'CREATED',
      dataType: 'date',
      caption: 'Created',
    }, {
      dataField: 'MODIFIED',
      dataType: 'date',
      caption: 'Modified',
    }, {
      dataField: 'CREATOR',
      caption: 'Created By',
      lookup: {},
    }, {
      dataField: 'STRUCTURE',
      dataType: 'string',
      allowFiltering: false,
      cellTemplate: 'cellTemplate',
      caption: 'Structure',
      width: 160,
    }]
  }
)();

export const RegistryFactory = makeTypedFactory<IRegistry, IRegistryRecord>({
  records: INITIAL_RECORDS,
  tempRecords: INITIAL_TEMP_RECORDS,
  temporary: true,
  currentId: -1,
  data: null,
  structureData: '',
});

export const INITIAL_STATE = RegistryFactory();
