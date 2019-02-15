import { trackTopmostSession, ITopmostSEssionInContainerChangedHandler } from "../src/index";
import * as chai from "chai";
import { JSDOM } from "jsdom";

chai.should();

describe.only("given a layout", () => {
    describe("the ui api", () => {
        it("can track the topmost session when scrolling", () => {
            let configuration = {
                hostOrigin: "https://docs.microsoft.com"
            };
            let dom = new JSDOM(
                `<!DOCTYPE html>
                <html lang="en">
                    <body>
                        <div id="container">
                            <div height="300px" style="margin:50px;" data-trydotnet-session-id="a"> div content a </div>
                            <div height="300px" style="margin:50px;" data-trydotnet-session-id="c"> div content c </div>
                            <div height="300px" style="margin:50px;" data-trydotnet-session-id="b"> div content b </div>
                            <div height="300px" style="margin:50px;" data-trydotnet-session-id="d"> div content d </div>
                            <div height="300px" style="margin:50px;" data-trydotnet-session-id="e"> div content e </div>
                        </div>
                    </body>
                </html>`,
                {
                    url: configuration.hostOrigin,
                    runScripts: "dangerously"
                });

          let frames = dom.window.document.querySelectorAll<HTMLIFrameElement>("iframe[data-trydotnet-session-id]");
          
        });
    });
});