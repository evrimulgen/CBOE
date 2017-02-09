import { IAppState } from '../../store';
import { FormGroupType, CFormGroup } from '../types/form.types';
import { DOMParser, DOMParserStatic, XMLSerializer } from 'xmldom';
import * as x2js from 'x2js';

export function getFormGroup(state: IAppState, type: FormGroupType): CFormGroup {
  let groups = (state.session.lookups.formGroups as Array<{name, data}>).filter(fg => fg.name === type.toString());
  if (!groups || groups.length === 0) {
    return null;
  }
  let doc = new DOMParser().parseFromString(groups[0].data);
  let x2jsTool = new x2js.default({
    arrayAccessFormPaths: [
      'formGroup.queryForms.queryForm',
      'formGroup.queryForms.queryForm.coeForms.coeForm',
      'formGroup.queryForms.queryForm.coeForms.coeForm.layoutInfor.formElement',
      'formGroup.queryForms.queryForm.coeForms.coeForm.addMode.formElement',
      'formGroup.queryForms.queryForm.coeForms.coeForm.editMode.formElement',
      'formGroup.queryForms.queryForm.coeForms.coeForm.viewMode.formElement',
      'formGroup.detailsForms.detailsForm',
      'formGroup.detailsForms.detailsForm.coeForms.coeForm',
      'formGroup.detailsForms.detailsForm.coeForms.coeForm.layoutInfor.formElement',
      'formGroup.detailsForms.detailsForm.coeForms.coeForm.addMode.formElement',
      'formGroup.detailsForms.detailsForm.coeForms.coeForm.editMode.formElement',
      'formGroup.detailsForms.detailsForm.coeForms.coeForm.viewMode.formElement',
      'formGroup.listForms.listForm',
      'formGroup.listForms.listForm.coeForms.coeForm',
      'formGroup.listForms.listForm.coeForms.coeForm.layoutInfor.formElement',
      'formGroup.listForms.listForm.coeForms.coeForm.addMode.formElement',
      'formGroup.listForms.listForm.coeForms.coeForm.editMode.formElement',
      'formGroup.listForms.listForm.coeForms.coeForm.viewMode.formElement'
    ]
  });
  return x2jsTool.dom2js(doc) as CFormGroup;
}
