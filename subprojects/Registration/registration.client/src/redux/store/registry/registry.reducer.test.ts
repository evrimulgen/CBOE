import { Iterable } from 'immutable';
import { RegistryFactory } from './registry.initial-state';
import { registryReducer } from './registry.reducer';
import { RegistryActions, RecordDetailActions } from '../../actions';
import { IRegistryRecord, IRecordDetail, CRecordsData } from './registry.types';

describe('registry reducer', () => {
  let initState: IRegistryRecord;

  beforeEach(() => {
    initState = RegistryFactory();
  });

  it('should have an immutable initial state', () => {
    expect(Iterable.isIterable(initState)).toBe(true);
  });

  it('should set record data on RETRIEVE_RECORD_SUCCESS', () => {
    const id = 100;
    const data = '<xml>encoded-temp-cdxml-data</xml>';
    const firstState = registryReducer(
      initState,
      RecordDetailActions.retrieveRecordSuccessAction({
        temporary: true,
        id: id,
        data: data
      } as IRecordDetail)
    );
    expect(firstState.currentRecord.temporary).toEqual(true);
    expect(firstState.currentRecord.id).toEqual(id);
    expect(firstState.currentRecord.data).toEqual(data);
    const id2 = 101;
    const data2 = '<xml>encoded-cdxml-data</xml>';
    const secondState = registryReducer(
      firstState,
      RecordDetailActions.retrieveRecordSuccessAction({
        temporary: false,
        id: id2,
        data: data2
      } as IRecordDetail)
    );
    expect(secondState.currentRecord.temporary).toEqual(false);
    expect(secondState.currentRecord.id).toEqual(id2);
    expect(secondState.currentRecord.data).toEqual(data2);
  });

  it('should ignore error cases', () => {
    const nextState = registryReducer(
      initState,
      RecordDetailActions.loadStructureErrorAction('error')
    );
    expect(nextState).toEqual(initState);
  });
});