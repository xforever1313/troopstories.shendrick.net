---
layout : page
title : Search for Stories
comments: false
icon: fa fa-search
pageindex: 4
---
@using Pretzel.Logic.Templating.Context
Search for Stories:

@Include( "search.md", Model, typeof( PageContext ) )
