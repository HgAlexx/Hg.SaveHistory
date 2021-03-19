local engine, ns = ...


ns.StringStartsWith = function(str, start)
   return str:sub(1, #start) == start
end

ns.StringEndsWith = function(str, ending)
   return ending == "" or str:sub(-#ending) == ending
end

ns.StringSplit = function(str, pattern)
    local result = {}
    for each in str:gmatch(pattern) do
        table.insert(result, each)
    end
    return result
end
