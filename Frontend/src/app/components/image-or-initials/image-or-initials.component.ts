import { Component, computed, input, signal, ChangeDetectionStrategy } from "@angular/core"

/** "Dra. Ana Souza" → "AS": first and last word, ignoring titles that end with a dot. */
export function initialsOf(text: string): string {
  const words = text.trim().split(/\s+/).filter((w) => w && !w.endsWith("."))
  if (words.length === 0) return "?"
  const first = words[0][0]
  const last = words.length > 1 ? words[words.length - 1][0] : ""
  return (first + last).toUpperCase()
}

/**
 * Shows an image, or the initials of `label` when there is no image or it fails to load
 * (deleted file, broken URL). Size and shape come from the classes passed in.
 */
@Component({
  selector: "app-image-or-initials",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    @if (showImage()) {
      <img [src]="src()" [alt]="alt()" [class]="imageClass()" loading="lazy" (error)="failedSrc.set(src())">
    } @else {
      <div [class]="placeholderClass()" role="img" [attr.aria-label]="alt()">{{ initials() }}</div>
    }
  `,
})
export class ImageOrInitialsComponent {
  src = input<string | null | undefined>()
  alt = input("")
  /** Text the initials are taken from (usually a person's or item's name). */
  label = input("")
  imageClass = input("")
  placeholderClass = input("flex items-center justify-center bg-green-100 text-green-700 font-semibold")

  protected failedSrc = signal<string | null | undefined>(undefined)
  protected showImage = computed(() => !!this.src() && this.src() !== this.failedSrc())
  protected initials = computed(() => initialsOf(this.label() || this.alt()))
}
