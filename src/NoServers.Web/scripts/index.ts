import * as $ from "jquery";
import * as Spinner from "spin";
import * as toastr from "toastr";
import * as api from "./api";
import * as data from "./data";
import * as security from "./security";
import * as util from "./util";

var spinner: Spinner;

function startWait() {
    const elem = document.getElementById("container");
    spinner = new Spinner().spin(elem);
}

function endWait() {
    spinner.stop();
}

function submitRegistration(form: any[]) {
    startWait();
    const reg = new api.Registration();
    reg.firstName = form["firstName"];
    reg.lastName = form["lastName"];
    reg.emailAddress = form["emailAddress"];
    reg.company = form["company"];
    reg.title = form["title"];
    data.getActiveEvent()
        .done(evt => {
            if (evt == null) {
                toastr.error("There doesn't seem to be an active event. Registration not saved.");
            } else {
                reg.eventId = evt.id;
                data.submitRegistration(reg)
                    .done(() => {
                        $("form").find("input[type='text'], input[type='email'], select").val("");
                        $("form").find("input[autofocus='autofocus']").focus();
                    });
            }
            toastr.success("Thank you for registering!");
        })
        .always(() => {
            endWait();
        });
}

$(() => {
    security.handleAuthentication();
});

$(() => {
    startWait();
    data.loadTitles()
        .done(titles => {
            const ddl = $("#title");
            ddl.append($("<option />").val("").text(""));
            titles.forEach(t => {
                ddl.append($("<option />").val(t.id).text(t.text));
            });
        })
        .always(() => endWait());
});

$(() => {
    const form = $("form");
    form.submit((e) => {
        e.preventDefault();
        submitRegistration(util.serializeForm(form));
    });

    const adminLink = $("#admin-link");
    adminLink.click((e) => {
        e.preventDefault();
        alert("Do fancy admin things!");
    });
});
