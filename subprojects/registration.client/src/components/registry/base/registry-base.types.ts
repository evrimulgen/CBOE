import { IFormGroup, IForm, ICoeForm, ICoeFormMode, IFormElement } from '../../../common';

export interface IViewControl {
  activated: boolean;
  editMode: boolean;
  viewModel: any;
  viewConfig: any;
}

export interface IFormItemTemplate extends IViewControl {
}

export interface IViewGroup {
  id: string;
  data: ICoeForm[];
}

export class CViewGroup implements IViewGroup {
  public id: string;
  public title: string;
  constructor(public data: ICoeForm[], private disabledControls: any[]) {
    this.update();
  }

  private update() {
    if (this.data.length === 1) {
      this.title = this.data[0].title;
      if (this.title) {
        this.id = this.title.toLowerCase().replace(/\s/g, '_');
      }
    }
  }

  private canAppend(f: ICoeForm): boolean {
    return this.data.length === 0 || !f.title || this.title === f.title;
  }

  private getFormElementContainer(f: ICoeForm, mode: string): ICoeFormMode {
    return mode === 'add' ? f.addMode : mode === 'edit' ? f.editMode : f.viewMode;
  }

  private setItemValue(item: any, property: string, value: any) {
    if (value) {
      item[property] = value;
    }
  }

  private getEditorType(fe: IFormElement): string {
    return fe.displayInfo.type.indexOf('COEDatePicker') > 0 ? 'dxDateBox' : 'dxTextBox';
  }

  private getDataField(fe: IFormElement): string {
    return fe._name.replace(/\s/g, '');
  }

  private getCellTemplate(fe: IFormElement): string {
    return fe.bindingExpression === 'ProjectList' ? 'projectsTemplate'
      : fe.displayInfo.type.endsWith('COEChemDraw') ? 'structureTemplate'
      // : fe.displayInfo.type.endsWith('COEChemDrawEmbedReadOnly') ? 'structureTemplate'
      : fe.displayInfo.type.endsWith('COEFragments') ? 'fragmentsTemplate'
      : undefined;
  }

  private checkDropDown(fe: IFormElement, item: any) {
    if (fe.displayInfo.type.endsWith('COEDropDownList')) {
      let pickListDomain: string = fe.configInfo ? fe.configInfo.fieldConfig.PickListDomain : undefined;
      if (pickListDomain && String(Math.floor(Number(pickListDomain))) === pickListDomain) {
        item.editorType = 'dxSelectBox';
      }
    }
  }

  public append(f: ICoeForm): boolean {
    let canAppend = this.canAppend(f);
    if (canAppend) {
      this.data.push(f);
      this.update();
    }
    return canAppend;
  }

  public getItems(displayMode: string): any[] {
    let items = [];
    this.data.forEach(f => {
      let formElementContainer = this.getFormElementContainer(f, displayMode);
      if (formElementContainer && formElementContainer.formElement) {
        formElementContainer.formElement.forEach(fe => {
          if (!this.disabledControls.find(dc => dc.id && dc.id === fe.Id) && fe.displayInfo && fe.displayInfo.visible === 'true' && fe._name) {
            let item: any = {};
            if (fe.label) {
              this.setItemValue(item, 'label', { text: fe.label });
            }
            this.setItemValue(item, 'editorType', this.getEditorType(fe));
            this.setItemValue(item, 'dataField', this.getDataField(fe));
            let template = this.getCellTemplate(fe);
            if (template) {
              this.setItemValue(item, 'template', template);
              if (template === 'structureTemplate' || template === 'fragmentsTemplate') {
                item.colSpan = 5;
              }
            }
            this.checkDropDown(fe, item);
            // if (item.template) {
            //   console.log(JSON.stringify(item));
            // }
            items.push(item);
          }
        });
      }
    });
    return items;
  }
}

export interface IParam {
  _name: string; // min
  _value: string; // 0
}

export class CParamList {
  param?: IParam[];
}

export interface IParamList extends CParamList {
}

export interface IValidationRule {
  _validationRuleName: string; // textLength
  _errorMessage: string; // The property value can have between 0 and 200 characters
  params?: IParamList;
}

export class CValidationRuleList {
  validationRule: IValidationRule[] = [];
}

export interface IValidationRuleList extends CValidationRuleList {
}

export interface IProperty {
  _name: string; // REG_COMMENTS
  _friendlyName: string; // REG_COMMENTS
  _type: string; // TEXT
  _precision?: string; // 200
  _sortOrder?: string; // 0
  _pickListDomainID?: string; // number
  _pickListDisplayValue?: string;
  validationRuleList?: IValidationRuleList;
  __text?: string;
}

export interface IPropertyList {
  Property: IProperty[];
}

export interface IRegNumber {
  RegID?: string; // 1826
  SequenceNumber?: string; // 877
  RegNumber?: string; // RN-000877
  SequenceID?: string; // 4
}

export interface IIdentifierID {
  _Description?: string;
  _Name?: string;
  _Active?: string; // T
  __text?: string; // 4
}

export interface IIdentifier {
  ID?: string; // 2621
  IdentifierID?: IIdentifierID;
  InputText?: string; // Dehydrohedione (DHH)
}

export class CIdentifierList {
  Identifier: IIdentifier[] = [];
}

export interface IIdentifierList extends CIdentifierList {
}

export interface IProjectID {
  _Description?: string; // Hedione Process Optimization
  _Name?: string; // Hedione Process Optimization
  _Active?: string; // T
  __text?: string; // 2
}

export interface IProject {
  ID?: string; // 381
  ProjectID?: IProjectID;
}

export class CProjectList {
  Project: IProject[] = [];
}

export interface IProjectList extends CProjectList {
}

export interface IChemicalStructure {
  _molWeight?: string; // 224.2961
  _formula?: string; // C13H20O3
  __text?: string; // VmpD...
}

export class IStructureData {
  StructureID?: string; // 1821
  StructureFormat?: string;
  Structure?: IChemicalStructure;
  NormalizedStructure?: string; // VmpD...
  UseNormalization?: string; // F
  DrawingType?: string; // 0
  CanPropogateStructureEdits?: string; // True
  PropertyList?: IPropertyList;
  IdentifierList?: IIdentifierList;
}

export interface IBaseFragment {
  Structure: IStructureData;
}

export class CFragment {
  FragmentID?: string; // number
  ComponentFragmentID?: string;
  Code?: string;
  FragmentType?: string;
  Description?: string;
  Structure?: string;
  DateCreated?: string; // date
  DateLastModified?: string; // date
}

export interface IFragment extends CFragment {
}

export class CFragmentList {
  Fragment: IFragment[] = [new CFragment()];
}

export interface IFragmentList extends CFragmentList {
}

export class CCompound {
  CompoundID?: string; // 901
  DateCreated?: string; // 2017-01-15 08:21:51 pm
  PersonCreated?: string; // 61
  PersonApproved?: string; // 61
  PersonRegistered?: string; // 61
  DateLastModified?: string; // 2017-01-15 08:38:18 pm
  Tag?: string; // P1
  PropertyList?: IPropertyList;
  RegNumber?: IRegNumber;
  CanPropogateComponentEdits?: string; // True
  FragmentList: IFragmentList = new CFragmentList();
  IdentifierList?: IIdentifierList;
}

export interface ICompound extends CCompound {
}

export class CComponent {
  ID?: string;
  ComponentIndex?: string; // -901
  Compound: ICompound = new CCompound();
}

export interface IComponent extends CComponent {
}

export class CComponentList {
  Component: IComponent[] = [];
}

export interface IComponentList extends CComponentList {
}

export interface IBatchComponentFragment {
  ID?: string; // number
  FragmentID?: string; // number
  ComponentFragmentID?: string; // number
  Equivalents?: string; // number
  OrderIndex?: string; // number
}

export class CBatchComponentFragmentList {
  BatchComponentFragment: IBatchComponentFragment[] = [];
}

export interface IBatchComponentFragmentList extends CBatchComponentFragmentList {
}

export interface IBatchComponent {
  ID?: string; // 1721
  BatchID?: string; // 1741
  CompoundID?: string; // 901
  MixtureComponentID?: string; // 901
  ComponentIndex?: string; // -901
  ComponentRegNum?: string; // C000885
  PropertyList?: IPropertyList;
  BatchComponentFragmentList?: IBatchComponentFragmentList;
}

export class CBatchComponentList {
  BatchComponent: IBatchComponent[] = [];
}

export interface IBatchComponentList extends CBatchComponentList {
}

export interface IPerson {
  _displayName?: string; // PMORIEUX
  __text?: string; // 61
}

export class CBatch {
  BatchID?: string; // 1741
  BatchNumber?: string; // 1
  FullRegNumber?: string; // RN-000877-001
  DateCreated?: string; // 2017-01-15
  PersonCreated?: IPerson;
  PersonApproved?: IPerson;
  PersonRegistered?: IPerson;
  DateLastModified?: string; // 2017-01-15
  StatusID?: string; // 3
  IsBatchEditable?: string; // True
  ProjectList?: IProjectList;
  IdentifierList?: IIdentifierList;
  PropertyList?: IPropertyList;
  BatchComponentList?: IBatchComponentList;
}

export interface IBatch extends CBatch {
}

export class CBatchList {
  Batch: IBatch[] = [];
}

export interface IBatchList extends CBatchList {
}

export interface IEvent {
  _eventName?: string; // Inserting
  _eventHandler?: string; // OnInsertHandler
}

export interface IAddIn {
  _assembly?: string; // CambridgeSoft.COE.Registration.RegistrationAddins, Version=12.1.0.0, Culture=neutral, PublicKeyToken=f435ba95da9797dc
  _class?: string; // CambridgeSoft.COE.Registration.Services.RegistrationAddins.NormalizedStructureAddIn
  _friendlyName?: string; // Structure Normalization
  _required?: string; // no
  _enabled?: string; // no
  Event?: IEvent[];
  AddInConfiguration?: any;
  // <AddInConfiguration>
  //   <ScriptFile>C:\Program Files\CambridgeSoft\ChemOfficeEnterprise12.1.0.0\Registration\PythonScripts\parentscript.py</ScriptFile>
  //   <!--Commented <PythonWebServiceURL> to bypass soap
  // <PythonWebServiceURL>http://localhost/PyEngine/Service.asmx</PythonWebServiceURL> -->
  //   <StructuresIdsToAvoid>-2|-3|</StructuresIdsToAvoid>
  // </AddInConfiguration>
}

export class CAddInList {
  AddIn: IAddIn[] = [];
}

export interface IAddInList extends CAddInList {
}

export class CRegistryRecord {
  _SameBatchesIdentity?: string; // True
  _ActiveRLS?: string; // Off
  _IsEditable?: string; // True
  _IsRegistryDeleteable?: string; // True
  ID?: string; // 921
  DateCreated?: string; // 2017-01-15 08:21:50 PM
  DateLastModified?: string; //  2017-01-15 08:38:18 PM
  PersonCreated?: string; // 61
  PersonApproved?: string; // number
  StructureAggregation?: string; // VmpD...
  StatusID?: string; // 3
  PropertyList?: IPropertyList;
  RegNumber?: IRegNumber;
  IdentifierList?: IIdentifierList;
  ProjectList?: IProjectList;
  ComponentList: IComponentList = new CComponentList();
  BatchList: IBatchList = new CBatchList();
  AddIns?: IAddInList;
  constructor() {
    this.ComponentList.Component.push(new CComponent());
    this.BatchList.Batch.push(new CBatch());
  }  
}

export interface IRegistryRecord extends CRegistryRecord {
}
