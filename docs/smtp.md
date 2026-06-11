# SMTP — Lecturer Onboarding Email

When an **Admin creates a Teacher / Lecturer account**, the Razor app sends that
person a **premium HTML welcome email** containing their login email, temporary
password, assigned course, and a login button.

![Teacher welcome email](images/email-teacher-welcome.png)

## Design & architecture

The email body is a real, responsive HTML email (table layout, inline CSS,
~600px wide, Gmail/Outlook friendly) — **not** plain text. Template concerns are
kept separate from both the transport and the business logic:

```
AcademicDocumentRagSystem.Services/
└── Email/
    ├── Templates/
    │   └── TeacherWelcome.html        # the HTML template (embedded resource)
    ├── Models/
    │   └── TeacherWelcomeEmailModel.cs # data only — no markup
    ├── IEmailTemplateRenderer.cs       # renders a model into HTML
    ├── EmailTemplateRenderer.cs        # loads template + substitutes {{tokens}}
    ├── SmtpSettings.cs                 # typed view of the "Smtp" config section
    └── (Interfaces/IEmailService.cs, Implementations/EmailService.cs)  # SMTP transport
```

* `TeacherWelcome.html` is shipped as an **embedded resource** (see the
  `<EmbeddedResource>` entry in the Services `.csproj`), so it is always available
  at runtime regardless of the working directory.
* Template variables: `{{TeacherName}}`, `{{Email}}`, `{{Password}}`,
  `{{CourseName}}`, `{{LoginUrl}}`, `{{CurrentYear}}`. Values are HTML-encoded.
* Adding a new email type = a new template + model + a render method. Localization
  can later swap the template per culture without touching senders.

## Who does what (responsibilities)

```
Accounts PageModel              AccountService                 IEmailTemplateRenderer / IEmailService
──────────────────              ──────────────                 ──────────────────────────────────────
OnPostCreateAsync()             CreateAsync(dto)
  → AccountService.CreateAsync    1. validate + INSERT account
  → reads CreateAccountResult     2. if role == Teacher:
  → shows success / warning          build TeacherWelcomeEmailModel
                                      html = renderer.RenderTeacherWelcome(model)
                                      emailService.SendEmailAsync(to, subject, html)
                                    3. return CreateAccountResult { EmailSent, EmailError }
```

* The **PageModel never sends email** — it calls the service.
* The **service** persists the account first, then sends the email.
* Email is sent **only for Teacher/Lecturer** accounts.

### Failure handling

If SMTP fails, the **account is not rolled back**. The exception is caught and
logged, and `CreateAccountResult.EmailSent = false` with the error message. The
PageModel then shows a yellow warning:

> *Account 'x@y.z' was created, but the notification email could not be sent. Reason: …*

So account creation never crashes the app, and the admin always knows whether
the email went out. (During screenshot capture, SMTP was intentionally left
blank — the demo lecturer account was still created and the warning was shown,
proving the safe-failure path.)

## Configuration

SMTP settings come from the `Smtp` section — **never hard-coded**. The shipped
`appsettings.json` uses blank placeholders:

```jsonc
"Smtp": {
  "Host": "",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "",
  "Password": "",
  "FromEmail": "",
  "FromName": "Academic Document RAG System"
},
"App": {
  "LoginUrl": "https://localhost:7150/Auth/Login"   // used as the email's login button target
}
```

### Recommended: user-secrets (do not commit real credentials)

```powershell
cd dotnet-razor/HoaiKhoi_SE1950_A02/AcademicDocumentRagSystem.RazorPages
dotnet user-secrets init
dotnet user-secrets set "Smtp:Host"      "smtp.gmail.com"
dotnet user-secrets set "Smtp:UserName"  "you@gmail.com"
dotnet user-secrets set "Smtp:Password"  "your-16-char-app-password"   # Gmail App Password, not your login
dotnet user-secrets set "Smtp:FromEmail" "you@gmail.com"
```

> Gmail requires an **App Password** (2FA enabled) — your normal password will be
> rejected. Outlook/Office365 and Mailtrap/SendGrid SMTP work the same way.

If `Smtp:Host` is blank, `EmailService` throws a clear "SMTP is not configured"
error, which is surfaced as the warning above (the account is still created).

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| "SMTP is not configured" warning | Populate the `Smtp` section (host + credentials). |
| Gmail "auth required" / 535 | Use an **App Password**, set `EnableSsl: true`, `Port: 587`. |
| Email created but never arrives | Check spam; verify `FromEmail` matches the authenticated mailbox. |
| Placeholders show as `{{Token}}` | Ensure `TeacherWelcome.html` is still an `<EmbeddedResource>` after edits. |
