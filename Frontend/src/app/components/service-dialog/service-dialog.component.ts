import { Component, type OnInit, type OnDestroy, inject, effect, ElementRef, ViewChild, AfterViewInit } from "@angular/core"
import { DialogService } from "../../services/dialog.service"
import { ViewportScroller } from "@angular/common"

@Component({
  selector: "app-service-dialog",
  standalone: true,
  templateUrl: "./service-dialog.component.html",
  styleUrl: "./service-dialog.component.css",
})
export class ServiceDialogComponent implements OnInit, OnDestroy, AfterViewInit {
  private dialogService = inject(DialogService)
  private viewportScroller = inject(ViewportScroller)
  private previousActiveElement: HTMLElement | null = null;

  @ViewChild('dialogContent', { static: false }) dialogContent?: ElementRef<HTMLElement>;
  @ViewChild('closeButton', { static: false }) closeButton?: ElementRef<HTMLButtonElement>;
  @ViewChild('firstFocusable', { static: false }) firstFocusable?: ElementRef<HTMLElement>;
  @ViewChild('lastFocusable', { static: false }) lastFocusable?: ElementRef<HTMLElement>;

  // Access the signals directly
  get isOpen() {
    return this.dialogService.isOpen$
  }

  get service() {
    return this.dialogService.service$
  }

  constructor() {
    // Use effect to react to signal changes
    effect(() => {
      if (this.isOpen()) {
        this.previousActiveElement = document.activeElement as HTMLElement;
        document.body.classList.add("overflow-hidden")
        // Focus will be set in ngAfterViewChecked or via setTimeout
        setTimeout(() => {
          this.closeButton?.nativeElement?.focus();
        }, 0);
      } else {
        document.body.classList.remove("overflow-hidden")
        // Restore focus to previous element
        if (this.previousActiveElement) {
          this.previousActiveElement.focus();
          this.previousActiveElement = null;
        }
      }
    })
  }

  ngOnInit(): void {
    // No need for subscriptions with signals
  }

  ngAfterViewInit(): void {
    // Focus management is handled in the effect
  }

  ngOnDestroy(): void {
    // Clean up even if the component is destroyed while dialog is open
    document.body.classList.remove("overflow-hidden")
    if (this.previousActiveElement) {
      this.previousActiveElement.focus();
    }
  }

  closeDialog(): void {
    this.dialogService.closeDialog()
  }

  scheduleConsultation(): void {
    this.closeDialog()
    setTimeout(() => {
      this.viewportScroller.scrollToAnchor("contact")
    }, 100)
  }

  /**
   * Handle keyboard events for accessibility
   */
  onKeyDown(event: KeyboardEvent): void {
    // Close dialog on Escape key
    if (event.key === 'Escape') {
      this.closeDialog();
      return;
    }

    // Trap focus within dialog
    if (event.key === 'Tab') {
      const focusableElements = this.dialogContent?.nativeElement.querySelectorAll(
        'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
      ) as NodeListOf<HTMLElement>;

      if (focusableElements.length === 0) return;

      const firstElement = focusableElements[0];
      const lastElement = focusableElements[focusableElements.length - 1];

      if (event.shiftKey) {
        // Shift + Tab
        if (document.activeElement === firstElement) {
          event.preventDefault();
          lastElement.focus();
        }
      } else {
        // Tab
        if (document.activeElement === lastElement) {
          event.preventDefault();
          firstElement.focus();
        }
      }
    }
  }
}
