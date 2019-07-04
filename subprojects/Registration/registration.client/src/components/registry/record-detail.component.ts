import { IInventoryContainerList } from './../../redux/store/registry/registry.types';
import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  OnInit, OnDestroy, OnChanges, AfterViewInit,
  ElementRef, ChangeDetectorRef, ViewEncapsulation,
  ViewChild, ViewChildren, QueryList, NgZone
} from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, UrlSegment, Params, Router } from '@angular/router';
import { Observable ,  Subscription } from 'rxjs';
import { select, NgRedux } from '@angular-redux/store';
import * as X2JS from 'x2js';
import { RecordDetailActions, IAppState, IRecordDetail, ILookupData } from '../../redux';
import * as registryUtils from './registry.utils';
import { IShareableObject, CShareableObject, IFormGroup, prepareFormGroupData, notify } from '../../common';
import { IResponseData, ITemplateData, CTemplateData, ICopyActions } from './registry.types';
import { DxFormComponent } from 'devextreme-angular';
import DxForm from 'devextreme/ui/form';
import { IRegistryRecord, CRegistryRecord, CViewGroup, RegRecordDetailBase } from './base';
import { basePath, apiUrlPrefix, invWideWindowParams } from '../../configuration';
import { FormGroupType, IFormContainer, getFormGroupData, notifyError, notifyException, notifySuccess } from '../../common';
import { HttpService } from '../../services';
import { RegTemplates } from './templates.component';
import { RegistryStatus } from './registry.types';
import { CFragment } from '../common';
import { PrivilegeUtils } from '../../common';
import { CSystemSettings, ISaveResponseData } from '../../redux';
import { RegInvContainerHandler } from './inventory-container-handler/inventory-container-handler';
import * as dxDialog from 'devextreme/ui/dialog';

@Component({
  selector: 'reg-record-detail',
  template: require('./record-detail.component.html'),
  styles: [require('./records.css')],
  host: { '(document:click)': 'onDocumentClick($event)' },
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegRecordDetail implements OnInit, OnDestroy, OnChanges {
  @ViewChild(RegTemplates) regTemplates: RegTemplates;
  @ViewChild(RegRecordDetailBase) recordDetailView: RegRecordDetailBase;
  @Input() temporary: boolean;
  @Input() template: boolean;
  @Input() id: number;
  @Input() bulkreg: boolean;
  @Input() useCurrent: boolean;
  @select(s => s.registry.duplicateRecords) duplicateRecord$: Observable<any[]>;
  @select(s => s.registry.saveResponse) saveResponse$: Observable<ISaveResponseData>;
  @select(s => s.registry.isLoading) isLoading$: Observable<any>;
  private displayMode: string;
  private title: string;
  private parentHeight: string;
  private approvalsEnabled: boolean = false;
  private editButtonEnabled: boolean = false;
  private saveButtonEnabled: boolean = false;
  private cancelButtonEnabled: boolean = false;
  private registerButtonEnabled: boolean = false;
  private approveButtonEnabled: boolean = false;
  private cancelApprovalButtonEnabled: boolean = false;
  private deleteButtonEnabled: boolean = false;
  private clearButtonEnabled: boolean = false;
  private submissionTemplatesEnabled: boolean = false;
  private routeSubscription: Subscription;
  private duplicateSubscription: Subscription;
  private saveResponseSubscription: Subscription;
  private loadingProgressSubscription: Subscription;
  private currentIndex: number = 0;
  private saveTemplateForm: DxForm;
  private saveTemplatePopupVisible: boolean = false;
  private newButtonEnabled: boolean = false;
  private backButtonEnabled: boolean = false;
  private revision; number = new Date().getTime();
  private copyActions: ICopyActions;
  private isDuplicatePopupVisible: boolean = false;
  private loadingVisible: boolean = false;
  private createContainerButtonEnabled: boolean = false;
  private invIntegrationEnabled: boolean = false;
  private sendToInventoryEnabled: boolean = false;
  private inventoryContainersList: IInventoryContainerList;
  private saveTemplatePopupHeight: number = 220;
  private showApprovedIcon: boolean = false;

  private saveTemplateItems = [{
    dataField: 'name',
    label: { text: 'Template Name' },
    dataType: 'string',
    editorType: 'dxTextBox',
    validationRules: [{ type: 'required', message: 'Name is required' }]
  }, {
    dataField: 'description',
    label: { text: 'Template Description' },
    dataType: 'string',
    editorType: 'dxTextArea'
  }, {
    dataField: 'isPublic',
    label: { text: 'Public Template' },
    dataType: 'boolean',
    editorType: 'dxCheckBox'
  }];
  private saveTemplateData: IShareableObject = new CShareableObject('', '', false);
  private isLoggedInUserOwner: boolean = false;
  private isLoggedInUserSuperVisor: boolean = false;

  constructor(
    public ngRedux: NgRedux<IAppState>,
    private elementRef: ElementRef,
    private router: Router,
    private http: HttpService,
    private location: Location,
    private actions: RecordDetailActions,
    private changeDetector: ChangeDetectorRef,
    private activatedRoute: ActivatedRoute,
    private ngZone: NgZone) {
  }

  ngOnInit() {
    const state = this.ngRedux.getState();
    if (this.id >= 0 && !this.useCurrent && (state == null || state.registry.currentRecord == null)) {
      return;
    }
    this.parentHeight = this.getParentHeight();
    if (this.routeSubscription == null) {
      this.routeSubscription = this.activatedRoute.url.subscribe((segments: UrlSegment[]) => this.initialize(segments));
    }
    if (this.loadingProgressSubscription == null) {
      this.loadingProgressSubscription = this.isLoading$.subscribe(d => { this.setProgressBarVisibility(d); });
    }
    if (this.duplicateSubscription == null) {
      this.duplicateSubscription = this.duplicateRecord$.subscribe((value) => this.duplicateData(value));
    }
    // Code for accessing "refreshRecordDetails()" from old UI
    window.NewRegWindowHandle = window.NewRegWindowHandle || {};
    window.NewRegWindowHandle.refreshRecordDetails = this.refreshRecordDetails.bind(this);
  }

  ngOnChanges() {
    this.update(false);
  }

  private update(forceUpdate: boolean = true) {
    let lookups = this.ngRedux.getState().session.lookups;
    if (!lookups || !lookups.userPrivileges || !this.recordDetailView || this.displayMode == null) {
      return;
    }
    let editMode = this.displayMode !== 'view';
    let userPrivileges = lookups.userPrivileges;
    let ss = new CSystemSettings(this.getLookup('systemSettings'));
    let statusId = this.statusId;
    let canEdit = this.isNewRecord ||
      PrivilegeUtils.hasEditRecordPrivilege(this.temporary, this.isLoggedInUserOwner, this.isLoggedInUserSuperVisor, userPrivileges);

    this.approvalsEnabled = (this.isNewRecord || this.temporary)
      && ss.isApprovalsEnabled
      && PrivilegeUtils.hasApprovalPrivilege(userPrivileges);

    this.cancelApprovalButtonEnabled = this.approvalsEnabled
      && !editMode
      && !!statusId
      && this.temporary
      && statusId === RegistryStatus.Approved
      && PrivilegeUtils.hasCancelApprovalPrivilege(userPrivileges);

    let enableEditIfNoteApproved: boolean = true;
    if (ss.isApprovalsEnabled) {
      enableEditIfNoteApproved = statusId === RegistryStatus.Approved ? false : true;
    }

    this.editButtonEnabled = !this.isNewRecord
      && enableEditIfNoteApproved
      && !editMode && canEdit;
    this.saveButtonEnabled = (this.isNewRecord && !this.cancelApprovalButtonEnabled) || editMode;
    this.cancelButtonEnabled = editMode && !this.isNewRecord;
    let canRegister = PrivilegeUtils.hasRegisterRecordPrivilege(this.isNewRecord, this.isLoggedInUserOwner, this.isLoggedInUserSuperVisor, userPrivileges);
    if (ss.isApprovalsEnabled) {
      canRegister = (statusId === RegistryStatus.Approved) && canRegister;
    }
    this.registerButtonEnabled = canRegister && (this.isNewRecord || (this.temporary && !editMode));

    this.approveButtonEnabled = !editMode && !!statusId && this.temporary && this.approvalsEnabled && statusId !== RegistryStatus.Approved;

    this.showApprovedIcon = !editMode && this.temporary && statusId === RegistryStatus.Approved;

    this.deleteButtonEnabled = !this.isNewRecord
      && PrivilegeUtils.hasDeleteRecordPrivilege(this.temporary, this.isLoggedInUserOwner, this.isLoggedInUserSuperVisor, userPrivileges)
      && this.editButtonEnabled;

    let canRedirectToTempListView = PrivilegeUtils.hasSearchTempPrivilege(this.ngRedux.getState().session.lookups.userPrivileges);
    this.clearButtonEnabled = this.isNewRecord;
    this.newButtonEnabled = this.temporary && !canRedirectToTempListView && !editMode;
    this.submissionTemplatesEnabled = this.isNewRecord
      && PrivilegeUtils.hasSubmissionTemplatePrivilege(userPrivileges) && ss.isSubmissionTemplateEnabled;
    let state = this.ngRedux.getState();
    this.invIntegrationEnabled = ss.isInventoryIntegrationEnabled
      && !this.temporary
      && !this.isNewRecord
      && !editMode;
    this.createContainerButtonEnabled = this.invIntegrationEnabled
      && ss.isSendToInventoryEnabled
      && PrivilegeUtils.hasCreateContainerPrivilege(userPrivileges);
    this.sendToInventoryEnabled = this.createContainerButtonEnabled;

    this.backButtonEnabled = !editMode;
    if (forceUpdate) {
      this.loadingVisible = false;
      this.changeDetector.markForCheck();
    }
  }

  initialize(segments: UrlSegment[]) {
    let newIndex = segments.findIndex(s => s.path === 'new');
    if (newIndex >= 0 && newIndex < segments.length - 1) {
      this.id = +segments[segments.length - 1].path;
    }
  }

  duplicateData(e) {
    if (e) {
      this.loadingVisible = false;
      if (e.TotalDuplicateCount) {
        // if duplicate records returned after clicking the duplicate action (continue) from popup window,
        // make sure that duplicate popup is hidden before displaying duplicate resolution options
        this.isDuplicatePopupVisible = false;
        this.currentIndex = 2;
        this.changeDetector.markForCheck();
      }
      if (e.copyActions) {
        this.isDuplicatePopupVisible = true;
        this.copyActions = e.copyActions;
        this.displayMode = 'edit';
      }
    }
  }

  ngOnDestroy() {
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
    if (this.duplicateSubscription) {
      this.duplicateSubscription.unsubscribe();
    }
    this.actions.clearSaveResponse();
    this.clearSaveResponseSubscription();

    if (this.loadingProgressSubscription) {
      this.loadingProgressSubscription.unsubscribe();
    }
    window.NewRegWindowHandle.refreshRecordDetails = null;
  }

  private getParentHeight() {
    return ((this.elementRef.nativeElement.parentElement.clientHeight) - 100).toString();
  }

  private onResize(event: any) {
    this.parentHeight = this.getParentHeight();
  }

  private onDocumentClick(event: any) {
    const target = event.target || event.srcElement;
    if (target.title === 'Full Screen') {
      let fullScreenMode = target.className === 'fa fa-compress fa-stack-1x white';
      this.parentHeight = (this.elementRef.nativeElement.parentElement.clientHeight - (fullScreenMode ? 10 : 190)).toString();
    }
  }

  getElementValue(e: Element, path: string) {
    return registryUtils.getElementValue(e, path);
  }

  cancel() {
    this.recordDetailView.clear();
    this.recordDetailView.prepareRegistryRecord();
    this.displayMode = 'view';
    this.update();
  }

  cancelDuplicateResolution(e) {
    if (e === 'cancel') {
      this.actions.clearDuplicateRecord();
      this.currentIndex = 0;
    } else {
      this.getSaveResponse();
    }
  }

  edit() {
    this.displayMode = 'edit';
    this.update();
  }

  newRecord() {
    this.router.navigate([`records/new?${new Date().getTime()}`]);
  }

  back() {
    if (this.bulkreg) {
      this.router.navigate([`records/bulkreg`]);
    } else {
      this.location.back();
    }
  }

  save(type?: string) {
    if (this.recordDetailView.save(type)) {
      this.getSaveResponse();
    }
  }

  register() {
    if (this.recordDetailView.register()) {
      this.getSaveResponse();
    }
  }

  getSaveResponse() {
    this.loadingVisible = true;
    if (!this.saveResponseSubscription) {
      this.saveResponseSubscription = this.saveResponse$.subscribe((value: ISaveResponseData) => this.refreshDetailView(value));
    }
  }

  private clearSaveResponseSubscription() {
    if (this.saveResponseSubscription) {
      this.saveResponseSubscription.unsubscribe();
      this.saveResponseSubscription = undefined;
    }
  }

  private refreshDetailView(data: ISaveResponseData, cancel?: boolean) {
    this.isDuplicatePopupVisible = false;
    if (data || cancel) {
      this.loadingVisible = false;
      // do not redirect to view mode, if there is a error returned from server api
      if (data && data.error) {
        this.changeDetector.markForCheck();
        return;
      }

      if (data && data.duplicateRecordCreationSuccess) {
        this.currentIndex = 0;
        if (this.id !== data.id) {
          // use case: new duplicate record is created via 'Move Batches' option
          // view should be refreshed with new record details in this case
          this.displayMode = 'view';
          this.id = data.id;
          this.temporary = data.temporary;
          // show load indicator while view is refreshing
          this.loadingVisible = true;
          return;
        }
      }

      this.displayMode = 'view';
      if (this.isNewRecord) {
        if (this.recordDetailView.displayMode !== 'view') {
          return;
        }
      }

      this.revision = new Date().getTime();
      this.update();
    }
  }

  private showSaveTemplate(e) {
    if (!this.recordDetailView.validate(true)) {
      return;
    }
    if (this.template) {
      let templateDialog = dxDialog.custom({
        title: 'Confirm overwrite template',
        message: 'Do you want to overwrite the saved template?',
        buttons: [{ text: 'Yes', onClick: () => { this.updateTemplate(); } },
        { text: 'No', onClick: () => { this.showSaveTemplatePopup(); } },
        { text: 'Cancel' }]
      });
      templateDialog.show();
    } else {
      this.showSaveTemplatePopup();
    }
  }

  private showSaveTemplatePopup() {
    this.saveTemplatePopupHeight = !!navigator.userAgent.match(/firefox/i) ? 238 : 220;
    this.saveTemplatePopupVisible = true;
  }

  private saveTemplate(e) {
    let result: any = this.saveTemplateForm.validate();
    if (result.isValid) {
      let recordDoc = this.recordDetailView.getUpdatedRecord();
      if (!recordDoc) {
        return;
      }
      let url = `${apiUrlPrefix}templates`;
      let data: ITemplateData = new CTemplateData(this.saveTemplateData.name);
      data.description = this.saveTemplateData.description;
      data.isPublic = this.saveTemplateData.isPublic;
      data.data = registryUtils.serializeData(recordDoc);
      this.loadingVisible = true;
      this.http.post(url, data).toPromise()
        .then(res => {
          this.regTemplates.dataSource = undefined;
          this.clearLoadIndicator();
          notifySuccess((res.json() as IResponseData).message, 5000);
        })
        .catch(error => {
          this.clearLoadIndicator();
          notifyException(`The submission data was not saved properly due to a problem`, error, 5000);
        });
      this.saveTemplatePopupVisible = false;
    }
  }

  private updateTemplate() {
    if (this.recordDetailView.validate()) {
      let recordDoc = this.recordDetailView.getUpdatedRecord();
      if (!recordDoc) {
        return;
      }
      let url = `${apiUrlPrefix}templates/${this.id}`;
      let data: ITemplateData = new CTemplateData(null);
      data.data = registryUtils.serializeData(recordDoc);
      this.loadingVisible = true;
      this.http.put(url, data).toPromise()
        .then(res => {
          this.regTemplates.dataSource = undefined;
          this.clearLoadIndicator();
          notifySuccess((res.json() as IResponseData).message, 5000);
        })
        .catch(error => {
          this.clearLoadIndicator();
          notifyException(`The submission data was not saved properly due to a problem`, error, 5000);
        });
      this.saveTemplatePopupVisible = false;
    }
  }

  clearLoadIndicator() {
    this.loadingVisible = false;
    this.changeDetector.markForCheck();
  }

  setProgressBarVisibility(e) {
    this.loadingVisible = e;
    this.changeDetector.markForCheck();
  }

  private cancelSaveTemplate(e) {
    this.saveTemplatePopupVisible = false;
  }

  private showTemplates(e) {
    this.currentIndex = 1;
    if (!this.regTemplates.dataSource) {
      this.regTemplates.loadData();
    }
    this.update();
  }

  private showDetails(e) {
    this.currentIndex = 0;
    this.update();
  }

  private get isNewRecord(): boolean {
    return this.id < 0 || this.template;
  }

  private get approvalIconEnabled(): boolean {
    let lookups = this.ngRedux.getState().session.lookups;
    if (!lookups) {
      return false;
    }

    let ss = new CSystemSettings(this.getLookup('systemSettings'));
    return this.temporary && this.displayMode === 'view' && ss.isApprovalsEnabled;
  }


  private get saveButtonTitle(): string {
    return this.isNewRecord ? 'Submit' : 'Save';
  }

  private onSaveTemplateFormInit(e) {
    this.saveTemplateForm = e.component as DxForm;
  }

  private get statusId(): number {
    return this.recordDetailView != null ? this.recordDetailView.statusId : null;
  }

  private set statusId(statusId: number) {
    if (this.recordDetailView != null) {
      this.recordDetailView.statusId = statusId;
    }
  }

  private cancelApproval() {
    let url = `${apiUrlPrefix}temp-records/${this.id}/${RegistryStatus.Submitted}`;
    this.loadingVisible = true;
    this.http.put(url, undefined).toPromise()
      .then(res => {
        this.regTemplates.dataSource = undefined;
        this.statusId = RegistryStatus.Submitted;
        this.update();
        this.clearLoadIndicator();
        notifySuccess(`The current temporary record's approval was cancelled successfully!`, 5000);
      })
      .catch(error => {
        this.clearLoadIndicator();
        notifyException(`The approval cancelling process failed due to a problem`, error, 5000);
      });
    this.saveTemplatePopupVisible = false;
  }

  private approve() {
    let url = `${apiUrlPrefix}temp-records/${this.id}/${RegistryStatus.Approved}`;
    this.loadingVisible = true;
    this.http.put(url, undefined).toPromise()
      .then(res => {
        this.regTemplates.dataSource = undefined;
        this.statusId = RegistryStatus.Approved;
        this.update();
        this.clearLoadIndicator();
        notifySuccess(`The current temporary record was approved successfully!`, 5000);
      })
      .catch(error => {
        this.clearLoadIndicator();
        notifyException(`The approval process failed due to a problem`, error, 5000);
      });
    this.saveTemplatePopupVisible = false;
  }

  private delete() {
    let dialogResult = dxDialog.confirm(
      `Are you sure you want to delete this Registry Record?`,
      `Confirm Delete`);
    dialogResult.done(result => {
      if (result) {
        this.loadingVisible = true;
        let url = `${apiUrlPrefix}${this.temporary ? 'temp-' : ''}records/${this.id}`;
        this.http.delete(url).toPromise()
          .then(res => {
            this.clearLoadIndicator();
            notifySuccess(`The record was deleted successfully!`, 5000);
            if (this.bulkreg) {
              this.router.navigate([`records/bulkreg`]);
            } else {
              const hitListId = Number(sessionStorage.searchHitlistId ? sessionStorage.searchHitlistId : '0');
              this.router.navigate([`records/${this.temporary ? 'temp' : ''}${hitListId > 0 ? '/hits/' + hitListId : ''}`]);
            }
          })
          .catch(error => {
            this.clearLoadIndicator();
            notifyException(`The record was not deleted due to a problem`, error, 5000);
          });
      }
    });
  }

  private clear() {
    if (this.template) {
      this.newRecord();
    } else {
      this.recordDetailView.clear();
    }
  }

  private getLookup(name: string): any[] {
    let lookups = this.ngRedux.getState().session.lookups;
    return lookups ? lookups[name] : [];
  }

  private onDetailContentReady(e) {
    let recordDetail: IRecordDetail = e.data;
    this.isLoggedInUserOwner = recordDetail.isLoggedInUserOwner;
    this.isLoggedInUserSuperVisor = recordDetail.isLoggedInUserSuperVisor;

    if (recordDetail.inventoryContainers && recordDetail.inventoryContainers.containers) {
      this.inventoryContainersList = recordDetail.inventoryContainers;
    }
    let recordDetailBase: RegRecordDetailBase = e.component;
    this.displayMode = recordDetailBase.displayMode;
    let recordId = recordDetailBase.recordId;
    this.title = this.isNewRecord ?
      'Register a New Compound' :
      `${this.temporary ? 'Temporary' : 'Registry'} Record: ${recordId}`;
    this.update();
  }

  private createCopies(e) {
    if (e === 'cancel') {
      this.refreshDetailView(null, true);
    } else {
      this.save(e);
    }
  }

  private createInvContainer() {
    let regInvContainer = new RegInvContainerHandler();
    let systemSettings = new CSystemSettings(this.getLookup('systemSettings'));
    systemSettings.isInventoryUseFullContainerForm
      ? regInvContainer.openContainerPopup((systemSettings.invNewContainerURL + `&vRegBatchID=` +
        this.recordDetailView.selectedBatchId + `&RefreshOpenerLocation=true`), null)
      : regInvContainer.openContainerPopup((systemSettings.invSendToInventoryURL + `?RegIDList=` +
        this.recordDetailView.id + `&OpenAsModalFrame=true`), invWideWindowParams);
  }

  refreshRecordDetails() {
    this.ngZone.run(() => this.refreshDetailView(null, true));
  }
}
