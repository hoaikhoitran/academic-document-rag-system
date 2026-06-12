# SignalR — Real-time Course Updates

The Razor Pages app (Assignment 02) adds **one** SignalR feature:

> When an **Admin** creates, updates, or deletes a Course, every **Teacher /
> Lecturer** currently viewing the live course list sees the change **without
> refreshing the page**.

This is verified end-to-end: with an admin in one browser and a lecturer in
another, creating and deleting a course updates the lecturer's table live (see
[`screenshots.md`](screenshots.md) → *Teacher live courses*).

## Moving parts

| Piece | Location |
| --- | --- |
| Hub | `AcademicDocumentRagSystem.RazorPages/Hubs/CourseHub.cs` |
| Hub registration + route | `Program.cs` → `AddSignalR()` + `MapHub<CourseHub>("/hubs/courses")` |
| Broadcast trigger | `Pages/Courses/Index.cshtml.cs` (after each successful CRUD) |
| Client script | `wwwroot/js/course-realtime.js` |
| Client library | `wwwroot/lib/signalr/dist/browser/signalr.min.js` (vendored locally) |
| Subscriber page | `Pages/Teacher/Courses.cshtml` (`#course-list` + `?handler=Table`) |

## Flow

```
Admin Courses page (PageModel)                 Lecturer "Live Courses" page (browser)
─────────────────────────────                 ──────────────────────────────────────
OnPostCreate/Edit/Delete                        connection = HubConnectionBuilder()
  → CourseService.<Crud>()  ── DB write ──┐         .withUrl("/hubs/courses")
  → (only on success)                     │         .withAutomaticReconnect()
      IHubContext<CourseHub>              │     connection.on("CoursesChanged", refresh)
        .Clients.All.SendAsync(           │
            "CourseCreated" | ...,         │     refresh():
            payload)                       └──►   fetch("/Teacher/Courses?handler=Table")
        .Clients.All.SendAsync(                     → swap #course-list innerHTML
            "CoursesChanged")                       → no full page reload
```

### Server: broadcast only after a successful write

```csharp
await _courseService.CreateAsync(CreateInput);           // 1. service-layer write
await _courseHub.Clients.All.SendAsync(                   // 2. specific event (optional)
    CourseHub.CourseCreated, new { CreateInput.Code, CreateInput.Name });
await _courseHub.Clients.All.SendAsync(CourseHub.CoursesChanged); // 3. coarse event
```

The PageModel — not the service — broadcasts, so the `Services` layer stays free
of any SignalR dependency. Events: `CourseCreated`, `CourseUpdated`,
`CourseDeleted`, and the coarse `CoursesChanged` that the client listens to.

### Client: refresh only the table fragment

`course-realtime.js` subscribes to `CoursesChanged` and re-fetches **only** the
course table partial (`Pages/Shared/_CourseTable.cshtml`) via the page's
`OnGetTableAsync` handler, then swaps it into `#course-list`. The rest of the
page is never reloaded. The connection uses `withAutomaticReconnect()`.

## Try it yourself

1. Run the Razor app and open `/Teacher/Courses` as a lecturer.
2. In another browser/incognito window, log in as Admin and create or delete a
   course at `/Courses/Index`.
3. The lecturer's list updates within a second — no manual refresh.

## Troubleshooting

| Symptom | Likely cause / fix |
| --- | --- |
| List does not update | Check the browser console; ensure `signalr.min.js` and `course-realtime.js` both load (200) and `/hubs/courses` negotiate succeeds. |
| `signalR is not defined` | The vendored client failed to load — confirm `wwwroot/lib/signalr/dist/browser/signalr.min.js` exists. |
| Updates only after manual refresh | The broadcast did not fire — verify the admin CRUD actually succeeded (a duplicate course code, for example, throws and does not broadcast). |
