import { IInventoryContainerList } from './../../../../redux/store/registry/registry.types';
import { Component, EventEmitter, Input, Output, OnChanges, ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';
import { NgRedux } from '@angular-redux/store';
import validationEngine from 'devextreme/ui/validation_engine';
import { CViewGroup, CViewGroupContainer, IRegistryRecord } from '../registry-base.types';
import { IViewControl } from '../../../common';
import { IFormGroup, IForm, ICoeForm } from '../../../../common';
import { IAppState } from '../../../../redux';

@Component({
  selector: 'reg-form-group-view',
  template: require('./form-group-view.component.html'),
  styles: [require('../registry-base.css')],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegFormGroupView implements IViewControl, OnChanges {
  @Input() id: string;
  @Input() activated: boolean;
  @Input() editMode: boolean;
  @Input() template: boolean;
  @Input() displayMode: string = 'add';
  @Input() viewModel: any;
  @Input() viewConfig: CViewGroupContainer[];
  @Input() updatable: boolean = false;
  @Input() invIntegrationEnabled: boolean = false;
  @Output() valueUpdated: EventEmitter<any> = new EventEmitter<any>();
  @Input() invContainers: IInventoryContainerList;

  constructor(private ngRedux: NgRedux<IAppState>) {
  }

  ngOnChanges() {
    this.update();
  }

  protected update() {
  }

  protected onValueUpdated(e) {
    this.valueUpdated.emit(this);
  }

  validate() {
    let result = validationEngine.validateGroup('vg');
    return result;
  }

  private get inventoryContainersViewEnabled(): boolean {
    return this.invIntegrationEnabled
      && this.displayMode === 'view'
      && this.invContainers && this.invContainers.containers
      && this.invContainers.containers.length > 0 ? true : false;
  }
};
