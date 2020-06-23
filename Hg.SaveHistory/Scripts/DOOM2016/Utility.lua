local engine, ns = ...


ns.StringStartsWith = function(str, start)
   return str:sub(1, #start) == start
end

ns.StringEndsWith = function(str, ending)
   return ending == "" or str:sub(-#ending) == ending
end

ns.StringSplit = function(str, pattern)
    return str:gmatch(pattern)
end
