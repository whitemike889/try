
import { Redis } from "armgen";
import { ServiceBus } from "armgen";
import { ServerFarm } from "armgen";
import { WebSite } from "armgen";
import { ContainerRegistry } from "armgen";
import { DocumentDB } from "armgen";
import { ApplicationInsights, AiOptions } from "armgen";
import { Scheduler, Job } from "armgen";
import { WebCertificate } from "armgen";
import { Parameter } from "armgen";

import { paramref, compile } from "armgen";
import { writeFileSync } from "fs";
import { AzureEndpoint } from "armgen";
import { NestedTemplate } from "armgen";
import { SslBinding } from "armgen";

function sanitize_prefix(prefix: string): string {
    return prefix.toLowerCase().replace(/[^a-z]/g, "");
}

function generateInstance(location: string, production: boolean): { [fileName: string]: string } {
    location = sanitize_prefix(location);
    const serverFarm = new ServerFarm(`${location}_tdna_sf`, { worker_count: 1, kind: "linux", resourceGroup: "trydotnet-agent-validation" });

    let containerRegistry: string | ContainerRegistry;
    let containerRegistryUrl: string;

    if (production) {
        containerRegistry = "trydotnetcdpx";
        containerRegistryUrl = `${containerRegistry}.azurecr.io`;
    } else {
        containerRegistry = new ContainerRegistry(`${location}tdncr`, { sku: "Basic", adminUserEnabled: true });
        containerRegistryUrl = containerRegistry.url();
    }

    const ai_options: AiOptions = { location: "West US 2" };
    const ai = new ApplicationInsights(`${location}_tdna_ai`, ai_options);

    const cdpxDockerUserName = new Parameter("cdpxdockerusername", "trydotnetcdpx user name", "securestring");
    const cdpxDockerPassword = new Parameter("cdpxdockerpassword", "trydotnetcdpx password", "securestring");
    const containerAppKey = new Parameter("containerappkey", "Service principal", "securestring");
    const parameters = [cdpxDockerUserName, cdpxDockerPassword, containerAppKey];

    const image = process.env["IMAGE"] as string;
    const tag = process.env["TAG"] as string;

    const webSiteName = `trydotnetagent-${location}`;
    const webSite = new WebSite(webSiteName, {
        serverFarm,
        docker: {
            containerRegistry,
            image: image,
            tag: tag,
            username: paramref(cdpxDockerUserName.name),
            password: paramref(cdpxDockerPassword.name),
        },
        kind: ["app", "linux", "container"],
        dependencies: [ai],
        connectionStrings: {
            "Insights_Key": ai.key(),
            "Insights_DeveloperMode": "false",
        },
        resourceGroup: "trydotnet-agent-validation",
        location: "East US"
    });

    return {
        "template.json": JSON.stringify(compile(parameters, webSite), null, 2),
    };
}

const [flag, user] = process.argv.splice(2);

if (flag === "--personal") {
    if (!user) {
        console.log("personal flag must be accompanied by a username");
        process.exit(-1);
    }
    console.log(generateInstance(user, false));
} else if (flag) {
    console.log(`unknown flag ${flag}`);
    process.exit(-1);
} else {
    let templates = generateInstance("westus", true);
    for (let fileName in templates) {
        writeFileSync(`../ServiceLayout/ServiceGroupRoot/Templates/${fileName}`, templates[fileName]);
    }
}
