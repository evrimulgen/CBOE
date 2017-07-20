import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { RegDataGridFormItem } from './data-grid-form-item';
import { RegFragmentsFormItem } from './fragments-form-item';
import { RegProjectsFormItem } from './projects-form-item';
import { RegStructureFormItem } from './structure-form-item';
import { RegTagBoxFormItem } from './tag-box-form-item';
import { RegFormView } from './form-view';
import { RegFormGroupView } from './form-group-view';
import { RegFormGroupItemView } from './form-group-item-view';
import {
  DxCheckBoxModule,
  DxRadioGroupModule,
  DxDataGridModule,
  DxDateBoxModule,
  DxSelectBoxModule,
  DxNumberBoxModule,
  DxFormModule,
  DxPopupModule,
  DxLoadIndicatorModule,
  DxLoadPanelModule,
  DxScrollViewModule,
  DxTextAreaModule,
  DxListModule,
  DxTagBoxModule,
  DxTextBoxModule,
  DxValidatorModule
} from 'devextreme-angular';
import { RegCommonModule } from '../../../common';
import { RegCommonComponentModule } from '../../common';

export * from './data-grid-form-item';
export * from './projects-form-item';
export * from './structure-form-item';
export * from './tag-box-form-item';
export * from './form-view';
export * from './form-group-view';
export * from './registry-base.types';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    DxCheckBoxModule,
    DxRadioGroupModule,
    DxDataGridModule,
    DxDateBoxModule,
    DxSelectBoxModule,
    DxNumberBoxModule,
    DxFormModule,
    DxPopupModule,
    DxLoadIndicatorModule,
    DxLoadPanelModule,
    DxScrollViewModule,
    DxTagBoxModule,
    DxTextAreaModule,
    DxListModule,
    DxTextBoxModule,
    DxValidatorModule,
    RegCommonModule,
    RegCommonComponentModule
  ],
  declarations: [
    RegDataGridFormItem, RegFragmentsFormItem, RegProjectsFormItem, RegStructureFormItem, RegTagBoxFormItem,
    RegFormView, RegFormGroupView, RegFormGroupItemView
  ],
  exports: [
    RegDataGridFormItem, RegFragmentsFormItem, RegProjectsFormItem, RegStructureFormItem, RegTagBoxFormItem,
    RegFormView, RegFormGroupView, RegFormGroupItemView,
    RegCommonModule, RegCommonComponentModule
  ]
})
export class RegBaseComponentModule { }
