import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ChemDrawWeb } from './chemdraw-web/chemdraw-web.component';
import { RegButton } from './button';
import { RegModal, RegModalContent } from './modal';

export * from './chemdraw-web/chemdraw-web.component';
export * from './button';
export * from './modal';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  declarations: [
    ChemDrawWeb,
    RegButton,
    RegModal, RegModalContent
  ],
  exports: [
    ChemDrawWeb,
    RegButton,
    RegModal, RegModalContent
  ]
})
export class RegCommonModule { }