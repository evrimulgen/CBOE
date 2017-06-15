import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Headers, RequestOptions } from '@angular/http';
import { UPDATE_LOCATION } from '@angular-redux/router';
import { NgRedux } from '@angular-redux/store';
import { Action, MiddlewareAPI } from 'redux';
import { createAction } from 'redux-actions';
import { Epic, ActionsObservable, combineEpics } from 'redux-observable';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/catch';
import * as registryUtils from '../components/registry/registry.utils';
import { apiUrlPrefix } from '../configuration';
import { notify, notifySuccess } from '../common';
import { IPayloadAction, RegActions, RegistryActions, RecordDetailActions, SessionActions } from '../actions';
import { IRecordDetail, IRegistry, IRegistryRetrievalQuery, IAppState } from '../store';
import { IResponseData, IRecordSaveData } from '../components';

@Injectable()
export class RegistryEpics {
  constructor(private http: Http, private ngRedux: NgRedux<IAppState>) { }

  handleRegistryActions: Epic = (action$: ActionsObservable, store: MiddlewareAPI<any>) => {
    return combineEpics(
      this.handleOpenRecords,
      this.handleRetrieveRecord,
      this.handleSaveRecord,
      this.handleLoadStructure,
    )(action$, store);
  }

 private handleOpenRecords: Epic = (action$: Observable<ReduxActions.Action<IRegistryRetrievalQuery>>) => {
    return action$.filter(({ type }) => type === RegistryActions.OPEN_RECORDS)
      .mergeMap(({ payload }) => {
        let url = `${apiUrlPrefix}${payload.temporary ? 'temp-' : ''}records`;
        let params = '';
        if (payload.skip) { params += `?skip=${payload.skip}`; }
        if (payload.take) { params += `${params ? '&' : '?'}count=${payload.take}`; }
        if (payload.sort) { params += `${params ? '&' : '?'}sort=${payload.sort}`; }
        url += params;
        return this.http.get(url)
          .map(result => {
            return result.url.indexOf('index.html') > 0
              ? SessionActions.logoutUserAction()
              : RegistryActions.openRecordsSuccessAction(payload.temporary, result.json());
          })
          .catch(error => Observable.of(RegistryActions.openRecordsErrorAction(error)));
      });
  }

  private handleRetrieveRecord: Epic = (action$: Observable<ReduxActions.Action<{ temporary: boolean, template: boolean, id: number }>>) => {
    return action$.filter(({ type }) => type === RecordDetailActions.RETRIEVE_RECORD)
      .mergeMap(({ payload }) => {
        let url = apiUrlPrefix;
        if (!payload.template) {
          url += `${payload.temporary ? 'temp-' : ''}records/${payload.id}`;
        } else {
          url += `templates/${payload.id}`;
        }
        return this.http.get(url)
          .map(result => {
            return result.url.indexOf('index.html') > 0
              ? SessionActions.logoutUserAction()
              : RecordDetailActions.retrieveRecordSuccessAction({
                temporary: payload.temporary,
                id: payload.id,
                data: result.json().data
              } as IRecordDetail);
          })
          .catch(error => Observable.of(RecordDetailActions.retrieveRecordErrorAction(error)));
      });
  }

  private handleSaveRecord: Epic = (action$: Observable<ReduxActions.Action<IRecordSaveData>>) => {
    return action$.filter(({ type }) => type === RecordDetailActions.SAVE_RECORD)
      .mergeMap(({ payload }) => {
        let id = payload.id;
        let createRecordAction = id < 0 || payload.saveToPermanent;
        let temporary = !payload.saveToPermanent && (id < 0 || payload.temporary);
        let data = registryUtils.serializeData(payload.recordDoc);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        return (createRecordAction
          ? this.http.post(`${apiUrlPrefix}${temporary ? 'temp-' : ''}records`, { data }, options)
          : this.http.put(`${apiUrlPrefix}${temporary ? 'temp-' : ''}records/${id}`, { data }, options))
          .map(result => {
            if (result.url.indexOf('index.html') > 0) {
              return SessionActions.logoutUserAction();
            } else {
              let responseData = result.json() as IResponseData;
              let actionType = payload.saveToPermanent ? 'registered' : 'saved';
              let newId = payload.saveToPermanent ? responseData.regNumber : responseData.id;
              newId = ` (${payload.saveToPermanent ? 'Reg Number' : 'ID'}: ${newId})`;
              let message = `The record was ${actionType} in the ${temporary ? 'temporary' : ''} registry`
                + `${createRecordAction ? newId : ''} successfully!`;
              notifySuccess(message, 5000);
              return createRecordAction
                ? createAction(UPDATE_LOCATION)(`records${temporary ? '/temp' : ''}`)
                : createAction(RegActions.IGNORE_ACTION)();
            }
          })
          .catch(error => Observable.of(RecordDetailActions.saveRecordErrorAction(error)));
      });
  }

  private handleLoadStructure: Epic = (action$: Observable<ReduxActions.Action<string>>) => {
    return action$.filter(({ type }) => type === RecordDetailActions.LOAD_STRUCTURE)
      .mergeMap(({ payload }) => {
        // Call CreateRegistryRecord
        let url: string = `${apiUrlPrefix}DataConversion/ToCdxml`;
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        let data: string = payload;
        return this.http.post(url, { data }, options)
          .map(result => {
            return result.url.indexOf('index.html') > 0
              ? SessionActions.logoutUserAction()
              : RecordDetailActions.loadStructureSuccessAction(result.json());
          })
          .catch(error => Observable.of(RecordDetailActions.loadStructureErrorAction(error)));
      });
  }
}
