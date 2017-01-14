import { Component, Input } from '@angular/core';

@Component({
  selector: 'reg-container',
  template: `
  <div
    [attr.data-testid]="testid"
    class="container-fluid pb4">
    <ng-content></ng-content>
  <div>
  `
})
export class RegContainer {
  @Input() testid: string;
};
