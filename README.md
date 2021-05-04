# SimpleRedirects
### Simple Redirect Manager for Umbraco 8 ###

This project is a fork of the original Simple301 version: https://github.com/wkallhof/Simple301

[![Build status](https://ci.appveyor.com/api/projects/status/j2ea8715x1ax8u9m/branch/master?svg=true)](https://ci.appveyor.com/project/patrickdemooij9/simpleredirects/branch/master)

Simple Redirects Manager is a simple to use, yet extensible, Umbraco Back-Office package that allows you to manage your 301 redirects directly in Umbraco. Includes a simple and intuitive refinement system where you can search for specific text within the URLs or notes defined for the redirect. 

Utilizes [ngTable][ngTableLink] for an AngularJs driven data table which includes ordering by column (Old Url, New Url, Notes and Last Updated) and simple pagination with configurable items per page selector. 

Integrates directly with the Umbraco Content Pipeline, inserting itself in the first position to intercept incoming requests and checking against an in-memory collection of redirects for optimal performance (the only time a database is hit is through updating redirects in the back-office and on application start.)

### Getting Started ###

Nuget Package: ` Install-Package SimpleRedirects `

### Configuration ###
You are able to change the configuration in your appsettings.json. By default the configuration is as follows:
![image](https://user-images.githubusercontent.com/11466511/117047797-9114ad00-ad12-11eb-8bbf-07a1566f6be9.png)

**SimpleRedirects.CacheDurationInSeconds** : This allows you to configure how long redirects are cached within the site. This only affects the user facing redirects so that the application doesn't read from the database for every request in the site. If you modify redirects within the back-office, the cache is automatically cleared per action (Add, Update, Delete). 

**SimpleRedirects.CacheEnabled** : This allows you to toggle whether or not caching is enabled. Since this package is hit for every requested URL in the site, it is important to consider the performance implications of disabling cache. Use this to troubleshoot redirect issues.

These caching settings were added in order to support load balanced environments, where in previous versions the applications held on to redirects only in memory (persisting to the DB only if modified), which doesn't work in a load balanced environement (they may exist in a memory collection on one server, but not on the other). 

### Usage ###

#### 1. Locate Simple Redirects Manager in the Content section ####
Navigate to the Umbraco > Content section. You will find a 'Manage Redirects' tab in the right pane. Select this to view the SimpleRedirects Redirect Manager. From this window you can view and manage all of the redirects for your site.
![Go to Umbraco > Content > Manage Redirects][locateImage]

#### 2. Refine & Locate ####
Use the provided Text search by entering text in the 'Search redirects' input box. As you type, results will display in real time. Use pagination and the results-per-page selectors to view more or fewer redirects at a time in the table.

![Refine by Text Search, and Pagination][refineImage]

### 3. Add, Update & Delete ###
Use the provided actions in the Action column to Add, Update & Delete existing redirects. Update your redirect in line with the rest of the redirects (make sure to click 'Save') or simply fill out a new redirect at the bottom of the table and click 'Add'. 

![Add, Update & Delete][crudImage]

__Support:__ [Documentation Wiki](https://github.com/patrickdemooij9/SimpleRedirects/wiki), [Issue Logging](https://github.com/patrickdemooij9/SimpleRedirects/issues)

[ngTableLink]: https://github.com/esvit/ng-table
[highlightJsLink]: https://github.com/isagalaev/highlight.js
[locateImage]: package/Locate.gif  "Locate"
[refineImage]: package/Refine.gif  "Refine"
[crudImage]: package/Crud.gif "Create, Update & Delete"
