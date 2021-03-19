--[[
    Based on sandbox 0.5 from:
    - https://github.com/APItools/sandbox.lua
    - https://github.com/xtofl/sandbox.lua

    Original by kikito Copyright below

    Modified by HgAlexx for the needs of Hg.SaveHistory and lua 5.3

--]]

local sandbox = {
  _VERSION      = "sandbox 0.6", -- bumped version to 0.6
  _DESCRIPTION  = "A pure-lua solution for running untrusted Lua code.",
  _URL          = "https://github.com/kikito/sandbox.lua",
  _LICENSE      = [[
    MIT LICENSE

    Copyright (c) 2013 Enrique Garc�a Cota

    Permission is hereby granted, free of charge, to any person obtaining a
    copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
    OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
  ]]
}


-- The base environment is merged with the given env option (or an empty table, if no env provided)
--
local BASE_ENV = {}

-- List of non-safe packages/functions:
--
-- * string.rep: can be used to allocate millions of bytes in 1 operation
-- * {set|get}metatable: can be used to modify the metatable of global objects (strings, integers)
-- * collectgarbage: can affect performance of other systems
-- * dofile: can access the server filesystem
-- * _G: It has access to everything. It can be mocked to other things though.
-- * load{file|string}: All unsafe because they can grant acces to global env
-- * raw{get|set|equal}: Potentially unsafe
-- * module|require|module: Can modify the host settings
-- * string.dump: Can display confidential server info (implementation of functions)
-- * string.rep: Can allocate millions of bytes in one go
-- * math.randomseed: Can affect the host sytem
-- * io.*, os.*: Most stuff there is non-safe

-- Safe packages/functions below
([[

_VERSION assert error    ipairs   next pairs
pcall    select tonumber tostring type unpack xpcall

coroutine.create coroutine.resume coroutine.running coroutine.status
coroutine.wrap   coroutine.yield

math.abs   math.acos math.asin  math.atan math.atan2 math.ceil
math.cos   math.cosh math.deg   math.exp  math.fmod  math.floor
math.frexp math.huge math.ldexp math.log  math.log10 math.max
math.min   math.modf math.pi    math.pow  math.rad   math.random
math.sin   math.sinh math.sqrt  math.tan  math.tanh

os.clock os.difftime os.time

string.byte string.char  string.find  string.format string.gmatch
string.gsub string.len   string.lower string.match  string.reverse
string.sub  string.upper

table.insert table.maxn table.remove table.sort

]]):gsub('%S+', function(id)
  local module, method = id:match('([^%.]+)%.([^%.]+)')
  if module then
    BASE_ENV[module]         = BASE_ENV[module] or {}
    BASE_ENV[module][method] = _ENV[module][method]
  else
    BASE_ENV[id] = _ENV[id]
  end
end)

local function protect_module(module, module_name)
  return setmetatable({}, {
    __index = module,
    __newindex = function(_, attr_name, _)
      error('Can not modify ' .. module_name .. '.' .. attr_name .. '. Protected by the sandbox.')
    end
  })
end

('coroutine math os string table debug'):gsub('%S+', function(module_name)
  BASE_ENV[module_name] = protect_module(BASE_ENV[module_name], module_name)
end)

-- auxiliary functions/variables

local string_rep = string.rep

local function merge(dest, source)
  for k,v in pairs(source) do
    dest[k] = dest[k] or v
  end
  return dest
end

local function sethook(f, key, quota)
  if type(debug) ~= 'table' or type(debug.sethook) ~= 'function' then return end
  debug.sethook(f, key, quota)
end

local function cleanup()
  sethook()
  string.rep = string_rep
end

local shared_env
local shared_quota = false

-- Public interface: sandbox.init
function sandbox.init(options)
  shared_env = {}
  options = options or {}

  if options.quota ~= false then
    shared_quota = options.quota or 500000
  end

  shared_env = merge(options.env or {}, BASE_ENV)
  shared_env._G = shared_env._G or shared_env
end

-- Public interface: sandbox.protect
function sandbox.protect(chunk, chunkname, options)
  if type(chunk) ~= 'string' then
    error('chunk must be a string')
  end
  if type(chunkname) ~= 'string' then
    error('chunkname must be a string')
  end

  local env
  local quota = false

  if options then
      if options.quota ~= false then
        quota = options.quota or 500000
      end
      env = merge(options.env or {}, BASE_ENV)
      env._G = env._G or env
  else
    quota = shared_quota
    env = shared_env
  end

  -- we allow only text chunk
  engineloader = load(chunk, chunkname, 't', env)

  return function(...)

    if quota then
      local timeout = function()
        cleanup()
        error('Quota exceeded: ' .. tostring(quota))
      end
      sethook(timeout, "", quota)
    end

    string.rep = nil

    local threadengine = coroutine.create(engineloader)
    local threadresults = {coroutine.resume(threadengine, ...)}
    local threadstatus = table.remove(threadresults, 1)

    cleanup()

    if not threadstatus then
        local message = ""
        if type(threadresults) == "string" then
            message = threadresults .. string.char(10)
        elseif type(threadresults) == "table" then
            if #threadresults == 1 then
                message = tostring(threadresults[#threadresults]) .. string.char(10)
            else
                for k, v in pairs(threadresults) do
                    message = message .. tostring(k) .. "=" .. tostring(v) .. string.char(10)
                end
            end
        else
            message = tostring(threadresults) .. string.char(10)
        end
        error(debug.traceback(threadengine, message))
    end

    return table.unpack(threadresults)
  end
end

-- Public interface: sandbox.run
function sandbox.run(chunk, chunkname, options, ...)
  return sandbox.protect(chunk, chunkname, options)(...)
end

return sandbox
