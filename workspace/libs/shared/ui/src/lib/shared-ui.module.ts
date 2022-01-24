import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { NgxMaskModule } from 'ngx-mask';

import { AlertActionsDirective } from './components/alert/alert-actions.directive';
import { AlertContentDirective } from './components/alert/alert-content.directive';
import { AlertComponent } from './components/alert/alert.component';
import { AnchorLinkComponent } from './components/anchor-link/anchor-link.component';
import { CardSummaryComponent } from './components/card-summary/card-summary.component';
import { CardActionsDirective } from './components/card/card-actions.directive';
import { CardContentDirective } from './components/card/card-content.directive';
import { CardHintDirective } from './components/card/card-hint.directive';
import { CardComponent } from './components/card/card.component';
import { CollectionNoticeComponent } from './components/collection-notice/collection-notice.component';
import { ContactFormComponent } from './components/contact-info-form/contact-info-form.component';
import { FormSectionComponent } from './components/form-section/form-section.component';
import { IconComponent } from './components/icon/icon.component';
import { KeyValueInfoComponent } from './components/key-value-info/key-value-info.component';
import { PageFooterComponent } from './components/page-footer/page-footer.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { PageSectionSubheaderDescDirective } from './components/page-section-subheader/page-section-subheader-desc.directive';
import { PageSectionSubheaderHintDirective } from './components/page-section-subheader/page-section-subheader-hint.directive';
import { PageSectionSubheaderComponent } from './components/page-section-subheader/page-section-subheader.component';
import { PageSectionComponent } from './components/page-section/page-section.component';
import { PageSubheaderComponent } from './components/page-subheader/page-subheader.component';
import { PageComponent } from './components/page/page.component';
import { PreferredNameFormComponent } from './components/preferred-name-form/preferred-name-form.component';
import { ToggleContentComponent } from './components/toggle-content/toggle-content.component';
import { UserInfoComponent } from './components/user-info/user-info.component';
import { YesNoContentComponent } from './components/yes-no-content/yes-no-content.component';
import { MaterialModule } from './material/material.module';
import { ContextHelpModule } from './modules/context-help/context-help.module';
import { DefaultPipe } from './pipes/default.pipe';
import { FormatDatePipe } from './pipes/format-date.pipe';
import { FullnamePipe } from './pipes/fullname.pipe';
import { PhonePipe } from './pipes/phone.pipe';
import { PostalPipe } from './pipes/postal.pipe';
import { SafePipe } from './pipes/safe.pipe';

@NgModule({
  declarations: [
    AlertComponent,
    AlertContentDirective,
    AlertActionsDirective,
    AnchorLinkComponent,
    CardComponent,
    CardHintDirective,
    CardContentDirective,
    CardActionsDirective,
    CardSummaryComponent,
    CollectionNoticeComponent,
    ContactFormComponent,
    FormSectionComponent,
    KeyValueInfoComponent,
    IconComponent,
    PageComponent,
    PageFooterComponent,
    PageHeaderComponent,
    PageSectionComponent,
    PageSectionSubheaderComponent,
    PageSectionSubheaderDescDirective,
    PageSectionSubheaderHintDirective,
    PageSubheaderComponent,
    PreferredNameFormComponent,
    ToggleContentComponent,
    UserInfoComponent,
    YesNoContentComponent,
    DefaultPipe,
    FormatDatePipe,
    FullnamePipe,
    PhonePipe,
    PostalPipe,
    SafePipe,
  ],
  imports: [
    CommonModule,
    ContextHelpModule,
    MaterialModule,
    NgxMaskModule.forRoot(),
    ReactiveFormsModule,
  ],
  exports: [
    ContextHelpModule,
    MaterialModule,
    NgxMaskModule,
    ReactiveFormsModule,
    AlertComponent,
    AlertContentDirective,
    AlertActionsDirective,
    AnchorLinkComponent,
    CardComponent,
    CardHintDirective,
    CardContentDirective,
    CardActionsDirective,
    CardSummaryComponent,
    CollectionNoticeComponent,
    ContactFormComponent,
    FormSectionComponent,
    KeyValueInfoComponent,
    IconComponent,
    PageComponent,
    PageFooterComponent,
    PageHeaderComponent,
    PageSectionComponent,
    PageSectionSubheaderComponent,
    PageSectionSubheaderDescDirective,
    PageSectionSubheaderHintDirective,
    PageSubheaderComponent,
    PreferredNameFormComponent,
    ToggleContentComponent,
    UserInfoComponent,
    YesNoContentComponent,
    DefaultPipe,
    FormatDatePipe,
    FullnamePipe,
    PhonePipe,
    PostalPipe,
    SafePipe,
  ],
})
export class SharedUiModule {}
