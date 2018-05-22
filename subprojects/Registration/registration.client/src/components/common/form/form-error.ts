import { Component, Input } from '@angular/core';

@Component({
  selector: 'reg-form-error',
  template: `
    <div
      [id]="qaid"
      [attr.data-testid]="testid"
      class="label label-danger"
      [ngClass]="{ 'display-none': !visible }">
      <ng-content></ng-content>
    </div>
  `
})
export class RegFormError {
  @Input() visible: boolean;
  @Input() qaid: string;
  @Input() testid: string = 'form-error';
};