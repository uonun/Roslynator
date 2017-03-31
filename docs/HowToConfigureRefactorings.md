# How to: Configure Refactorings

> There is a difference between analyzers and refactorings (Please see [Analyzers vs. Refactorings](AnalyzersVsRefactorings.md)). If you want to configure analyzers please see [How to Configure Analyzers](HowToConfigureAnalyzers.md).

## Introduction

Visual Studio does not provide any configuration mechanism for refactorings. Since it is desirable to enable/disable a given refactoring, Roslynator provides two way for configuring refactorings:

* Visual Studio Options Page
* Config File

## Visual Studio Options

* Roslynator provides standard options page that enables to disable a given refactoring.

![RefactoringsOptions](/images/RefactoringsOptions.png)

## Config File

* Config file provides advanced way for configuring refactorings.

### Benefits

* It is not bound to IDE installation.
* One configuration file can be used by many developers.

### Specification

Config file have to have name **roslynator.config** and has to be placed in solution root directory. It has to have following structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Roslynator>
  <Settings>
    <General>
      <PrefixFieldIdentifierWithUnderscore IsEnabled="true" />
    </General>
    <Refactorings>
      <Refactoring Id="RR0001" IsEnabled="false" />
    </Refactorings>
  </Settings>
</Roslynator>```

By default, any setting in config file OVERRIDES settings from IDE options. This behavior can be disabled by unchecking **Use config file** in the IDE options.

### Default Config File

Refactorings are distinguished by their identifiers which is not very descriptive. To make config file more desriptive, you can use [Default Config File](source/Refactorings/DefaultConfigFile.xml) which contains comment for each refactoring.
