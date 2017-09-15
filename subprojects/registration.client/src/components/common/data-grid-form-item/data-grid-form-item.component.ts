import { Component, EventEmitter, Input, Output, OnChanges, ChangeDetectionStrategy, ViewEncapsulation, ViewChild } from '@angular/core';
import { DxDataGridComponent } from 'devextreme-angular';
import { RegBaseFormItem } from '../base-form-item';

export const dataGridFormItemTemplate = require('./data-grid-form-item.component.html');

@Component({
  selector: 'reg-data-grid-form-item-template',
  template: dataGridFormItemTemplate,
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegDataGridFormItem extends RegBaseFormItem {
  @ViewChild(DxDataGridComponent) grid: DxDataGridComponent;
  protected dataSource: any[];
  protected columns: any[];
  protected editingMode: string;
  protected allowUpdating: boolean;
  protected allowDeleting: boolean;
  protected allowAdding: boolean;

  protected checkCommandColumn() {
    if (this.editMode && this.columns.length > 0 && !this.columns[0].headerCellTemplate) {
      this.columns.unshift({
        cellTemplate: 'commandCellTemplate',
        headerCellTemplate: 'commandHeaderCellTemplate',
        width: 80
      });
    } else if (!this.editMode && this.columns.length > 0 && this.columns[0].headerCellTemplate) {
      this.columns.splice(0, 1);
    }
  }

  protected update() {
    let options = this.viewModel.editorOptions;
    this.dataSource = options && options.value ? options.value : [];
    this.columns = options && options.columns ? options.columns : [];
    this.checkCommandColumn();
    this.editingMode = options && options.editing && options.editing.mode
      ? options.editing.mode
      : 'row';
    this.allowUpdating = options && options.editing && options.editing.allowUpdating
      ? options.editing.allowUpdating
      : false;
    this.allowDeleting = options && options.editing && options.editing.allowDeleting
      ? options.editing.allowDeleting
      : false;
    this.allowAdding = options && options.editing && options.editing.allowAdding
      ? options.editing.allowAdding
      : false;
  }

  protected onContentReady(e) {
    let grid = e.component;
    if (grid.getRowElement(0) == null) {
      grid.option('height', 60);
    } else {
      grid.option('height', 'auto');
    }
  }

  protected onRowInserting(e, d) {
  }

  protected onRowUpdating(e, d) {
  }

  protected onRowRemoving(e, d) {
  }

  protected onRowInserted(e, d) {
    this.onGridChanged(d.component);
  }

  protected onRowUpdated(e, d) {
    this.onGridChanged(d.component);
  }

  protected onRowRemoved(e, d) {
    this.onGridChanged(d.component);
  }

  protected addRow(e) {
    e.component.addRow();
  }

  protected edit(e) {
    if (this.allowUpdating) {
      e.component.editRow(e.row.rowIndex);
    }
  }

  protected delete(e) {
    if (this.allowDeleting) {
      e.component.deleteRow(e.row.rowIndex);
    }
  }

  protected save(e) {
    e.component.saveEditData();
  }

  protected cancel(e) {
    e.component.cancelEditData();
  }

  protected onGridChanged(component) {
    let value = this.serializeValue(this.dataSource);
    component.option('formData.' + this.viewModel.dataField, value);
    this.valueUpdated.emit(this);
  }

  protected onDropDownValueUpdated(e, d) {
    this.grid.instance.cellValue(d.rowIndex, d.column.dataField, e);
  }
};