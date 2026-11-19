import { Component, OnInit, inject, signal, DestroyRef } from "@angular/core"
import { NgClass, DatePipe } from "@angular/common"
import { takeUntilDestroyed } from "@angular/core/rxjs-interop"
import { CoursesService } from "../../services/courses.service"
import { MetaService } from "../../services/meta.service"
import { of } from "rxjs"
import { catchError } from "rxjs/operators"
import type { Course } from "../../models/course.model"
import { CourseStatus, getCourseStatusDisplayName } from "../../enums/course-status.enum"
import { FooterComponent } from "../../components/footer/footer.component"
import { ImageOrInitialsComponent } from "../../components/image-or-initials/image-or-initials.component"

@Component({
  selector: "app-courses",
  standalone: true,
  imports: [NgClass, DatePipe, FooterComponent, ImageOrInitialsComponent],
  templateUrl: "./courses.component.html",
  styleUrl: "./courses.component.css",
})
export class CoursesComponent implements OnInit {
  private coursesService = inject(CoursesService)
  private metaService = inject(MetaService)
  private destroyRef = inject(DestroyRef)

  courses = signal<Course[]>([])
  isLoading = signal(true)

  protected readonly CourseStatus = CourseStatus

  /** The API sends the status identifier ("InscricoesAbertas"); show the readable text. */
  statusText(course: Course): string {
    return getCourseStatusDisplayName(course.status)
  }

  /** Enrollment is possible (badge in green, "Saiba mais" link shown). */
  isOpen(course: Course): boolean {
    return course.status === CourseStatus.InscricoesAbertas || course.status === CourseStatus.Disponivel
  }

  ngOnInit(): void {
    // Update meta tags for courses page
    this.metaService.updateMetaTags({
      title: 'Cursos',
      description: 'Descubra nossos cursos e formações em saúde integrativa. Desenvolva novas habilidades e expanda seu conhecimento.',
    });
    this.coursesService
      .list()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError(() => {
          this.courses.set([])
          this.isLoading.set(false)
          return of([] as Course[])
        })
      )
      .subscribe((courses: Course[]) => {
        this.courses.set(courses)
        this.isLoading.set(false)
      })
  }

}


