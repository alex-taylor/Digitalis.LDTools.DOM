Digitalis.LDTools.DOM
=====================

A .NET implementation of the LDraw (www.ldraw.org) data-model.

This is still something of a work-in-progress; in particular, see TODO.txt for a list of the major outstanding areas, plus a few known minor issues.

To build, you'll need to pull both this repository and the Digitalis.UndoSystem repo; you may also want to pull the LDTools documentation as well.

Some of the unit-tests require an LDraw library (downloadable from www.ldraw.org) to be installed on your machine; to simplify setup this should be placed in C:\LDraw.  Some of the tests refer to files in the TestData folder, and - for the moment - use absolute filepaths which means that they won't pass unless you happen to have the same folder-structure on your machine as on mine.  This will be sorted out soon!


Status
------

In terms of completeness, this is what's done:

 - the main API: this covers all the standard LDraw element-types (line, quad, reference, comment etc) plus some of the extensions (Texmap, group)
 - an implementation of this API
 - various support classes, notably a system for managing an LDraw library installation

It's entirely usable as it stands; no major changes to the API are planned, and the outstanding work can be broken down into:

 - more tests!  Most of the API now has unit-tests, but there are still a few gaps in coverage (currently it's at around 85%) to be filled
 - some 'advanced' features which I'm still thinking about, such as:
    - the ability to define 'complex' elements such as a complete minifig
    - control-points (or control-handles, whatever I end up calling them): the ability for an element to declare a set of 'handles' which can be used to adjust its properties, deform it, etc
    - 'operations': elements could declare operations (probably implemented via delegates) that they support, rather than being limited to whatever I decide to build into the DOM
 - a number of minor bug-fixes and gaps in the implementation



Usage
-----

To get started, build the DLL and load an LDraw document like this:

    LDDocument doc = new LDDocument("path to my document", ParseFlags.None);

An LDDocument is a collection of LDPage objects, each of which is a collection of LDStep objects, each of which is a collection of LDElement objects - in short, it's a tree.  All the usual collections APIs are available on these types, along with a few LDraw-specific ones.  Everything's documented, so please take a look!



Highlights
----------

The DOM objects will - where appropriate - prevent you from making 'illegal' changes, such as creating a circular-reference or a duplicate group.  For changes which are syntactically valid but disallowed by the LDraw specifications, the Analytics system will provide feedback objects which describe the problem - both as a GUID for the benefit of software and as a user-friendly message - and where possible may also provide the means to correct the problem.  Take a look at the APIs in Analytics.cs, and the documentation for the Analyse() method on the individual element APIs for full details.



The LibraryManager class, along with its supporting IndexCard, provides a cacheing mechanism for the LDraw library installation.  On first access it will scan the specified LDraw folder and build a database of the available files - parts, subparts, primitives and models.  The full list of folders it searches are detailed in Configuration.cs.  This cache is persisted to Isolated Storage and will be reloaded automatically the next time the library is initialised.  If changes have occurred since the cache was last written - files being added, removed or modified - the LibraryManager will auto-detect this and update its cache.  It can also detect these changes after initialisation, and provides an event for applications to subscribe to in order to hear about these changes.

The main element-type, LDReference, supports this event and uses it to auto-update itself if the LibraryManager signals that the file being referenced has changed, been deleted - or been added if it wasn't present the last time the LDReference asked for it.



The entire DOM supports a sophisticated change-notification system.  Each public property of each element type has a corresponding 'changed' event, and in addition each type supports a generic 'something changed' event which may be subscribed to by applications which simply need to know that the element has changed in some way but don't care too much about the specifics - for example, in order to warn the user that their document has unsaved changes.  The collection elements - LDStep, LDPage, etc - forward the 'generic' events of their children on via an event of their own, so ultimately a program need only subscribe to the 'something changed' event on LDDocument in order to hear about every single change made at any level of the document-tree.  LDReference objects make use of this mechanism to react to and forward change-events from the LDPage they are referencing.



There is of course a lot more to it than that, and extensive documentation is provided in the source-files.  A .mshc file is provided in the Digitalis.LDTools.Documentation project if you'd prefer to install that.
