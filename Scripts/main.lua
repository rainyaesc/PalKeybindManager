-- PalKeybindManager 0.1.0-personal
-- Runtime-only input backend. Persistent overrides live only in this Mod directory.

local MOD = "PalKeybindManager"
local VERSION = "0.4.0-personal-cn-panel"

local function current_mod_root()
    local ok, source = pcall(function() return debug.getinfo(1, "S").source end)
    if not ok or type(source) ~= "string" then return nil end
    source = source:gsub("^@", "")
    return source:match("^(.*)[/\\]Scripts[/\\][^/\\]+$")
end

local MOD_ROOT = current_mod_root()
local CONFIG_PATH = MOD_ROOT and (MOD_ROOT .. "/config.tsv") or "PalKeybindManager-config.tsv"
local MAPPINGS_PATH = MOD_ROOT and (MOD_ROOT .. "/mappings.tsv") or "PalKeybindManager-mappings.tsv"
local LOG_PATH = MOD_ROOT and (MOD_ROOT .. "/runtime.log") or "PalKeybindManager-runtime.log"

local baseline = {}
local baseline_ready = false
local applying = false
local apply_pending = false
local apply_number = 0

local function append_line(path, message)
    local ok, handle = pcall(io.open, path, "a")
    if ok and handle then
        handle:write(tostring(message), "\n")
        handle:close()
    end
end

local function log(message)
    print(string.format("[%s] %s\n", MOD, tostring(message)))
    append_line(LOG_PATH, string.format("[%s] %s", os.date("!%Y-%m-%dT%H:%M:%SZ"), tostring(message)))
end

local function read_file(path)
    local ok, handle = pcall(io.open, path, "rb")
    if not ok or not handle then return "" end
    local content = handle:read("*a") or ""
    handle:close()
    return content
end

local function valid(value)
    if value == nil then return false end
    local ok, result = pcall(function() return value:IsValid() end)
    return ok and result == true
end

local function describe(value)
    if value == nil then return "nil" end
    if type(value) == "string" or type(value) == "number" or type(value) == "boolean" then return tostring(value) end
    local attempts = {
        function() return value:ToString() end,
        function() return value:GetFName():ToString() end,
        function() return value:GetFullName() end,
    }
    for _, attempt in ipairs(attempts) do
        local ok, result = pcall(attempt)
        if ok and result ~= nil then return tostring(result) end
    end
    return tostring(value)
end

local function input_settings()
    local ok, settings = pcall(function() return StaticFindObject("/Script/Engine.Default__InputSettings") end)
    if not ok or not valid(settings) then return nil, tostring(settings) end
    return settings, nil
end

local function array_info(owner, property_name)
    local ok, array = pcall(function() return owner[property_name] end)
    if not ok then return nil, nil, tostring(array) end
    local count_ok, count = pcall(function() return array:GetArrayNum() end)
    if not count_ok then return nil, nil, tostring(count) end
    return array, count, nil
end

local function mapping_name(mapping, kind)
    local field = kind == "A" and "ActionName" or "AxisName"
    local ok, value = pcall(function() return mapping[field] end)
    return ok and describe(value) or "<unreadable>"
end

local function mapping_key(mapping)
    local ok, value = pcall(function() return mapping.Key.KeyName end)
    return ok and describe(value) or "<unreadable>"
end

local function mapping_scale(mapping)
    local ok, value = pcall(function() return mapping.Scale end)
    return ok and tonumber(describe(value)) or nil
end

local function action_modifier(mapping, name)
    local ok, value = pcall(function() return mapping[name] end)
    return ok and value == true
end

local EXCLUDED_PREFIXES = {
    "Gamepad_", "Vive_", "Oculus", "MixedReality_", "Daydream_", "ValveIndex_",
    "MagicLeap_", "MotionController_", "Touch", "PS5_", "Steam_", "Virtual_"
}

local function is_pc_key(key)
    if key == "None" or key == "<unreadable>" then return false end
    for _, prefix in ipairs(EXCLUDED_PREFIXES) do
        if string.sub(key, 1, #prefix) == prefix then return false end
    end
    return true
end

local function is_editable_key(key)
    if not is_pc_key(key) then return false end
    if key == "MouseX" or key == "MouseY" or key == "MouseWheelAxis" then return false end
    return true
end

local function mapping_id(kind, index, name)
    return string.format("%s:%d:%s", kind, index, name)
end

local function set_mapping_key(mapping, key)
    return pcall(function() mapping.Key.KeyName = FName(key) end)
end

local function parse_config(text)
    local result = {}
    text = text:gsub("^\239\187\191", "")
    for line in string.gmatch(text, "[^\r\n]+") do
        if string.sub(line, 1, 1) ~= "#" then
            local id, key = line:match("^([^\t]+)\t([^\t]+)$")
            if id and key then result[id] = key end
        end
    end
    return result
end

local function each_mapping(settings, callback)
    local specs = {
        { kind = "A", property = "ActionMappings" },
        { kind = "X", property = "AxisMappings" },
    }
    for _, spec in ipairs(specs) do
        local array, count, array_error = array_info(settings, spec.property)
        if not array then error(spec.property .. " unavailable: " .. tostring(array_error)) end
        for index = 1, count do
            local ok, mapping = pcall(function() return array[index] end)
            if ok and mapping ~= nil then callback(spec.kind, index, mapping) end
        end
    end
end

local function capture_baseline(settings)
    baseline = {}
    each_mapping(settings, function(kind, index, mapping)
        local name = mapping_name(mapping, kind)
        local id = mapping_id(kind, index, name)
        baseline[id] = {
            key = mapping_key(mapping),
            name = name,
            kind = kind,
            index = index,
            scale = kind == "X" and mapping_scale(mapping) or nil,
        }
    end)
    baseline_ready = true
    local count = 0
    for _ in pairs(baseline) do count = count + 1 end
    log("captured runtime baseline mappings=" .. tostring(count))
end

local function modifiers_text(mapping)
    local values = {}
    if action_modifier(mapping, "bCtrl") then values[#values + 1] = "Ctrl" end
    if action_modifier(mapping, "bShift") then values[#values + 1] = "Shift" end
    if action_modifier(mapping, "bAlt") then values[#values + 1] = "Alt" end
    if action_modifier(mapping, "bCmd") then values[#values + 1] = "Cmd" end
    return table.concat(values, "+")
end

local function sanitize_tsv(value)
    return tostring(value or ""):gsub("[\t\r\n]", " ")
end

local function write_manifest(settings, overrides)
    local ok, handle = pcall(io.open, MAPPINGS_PATH, "wb")
    if not ok or not handle then
        log("ERROR cannot write mappings.tsv: " .. tostring(handle))
        return
    end
    handle:write("# PalKeybindManager mappings v1\n")
    handle:write("# id\tkind\tindex\tname\toriginal_key\tcurrent_key\tscale\tmodifiers\teditable\toverridden\n")
    each_mapping(settings, function(kind, index, mapping)
        local name = mapping_name(mapping, kind)
        local id = mapping_id(kind, index, name)
        local base = baseline[id]
        local original_key = base and base.key or mapping_key(mapping)
        local current_key = mapping_key(mapping)
        local editable = is_editable_key(original_key) and "1" or "0"
        local scale = kind == "X" and tostring(mapping_scale(mapping) or "") or ""
        local modifiers = kind == "A" and modifiers_text(mapping) or ""
        handle:write(table.concat({
            sanitize_tsv(id), sanitize_tsv(kind), tostring(index), sanitize_tsv(name),
            sanitize_tsv(original_key), sanitize_tsv(current_key), sanitize_tsv(scale),
            sanitize_tsv(modifiers), editable, overrides[id] and "1" or "0"
        }, "\t"), "\n")
    end)
    handle:close()
end

local function apply_config(reason)
    if applying then return end
    applying = true
    apply_pending = false
    apply_number = apply_number + 1

    local ok, error_message = pcall(function()
        local settings, settings_error = input_settings()
        if not settings then error("InputSettings unavailable: " .. tostring(settings_error)) end
        if not baseline_ready then capture_baseline(settings) end

        local config_text = read_file(CONFIG_PATH)
        local overrides = parse_config(config_text)
        local changed = 0
        local applied = 0
        local missing = {}
        for id in pairs(overrides) do missing[id] = true end

        each_mapping(settings, function(kind, index, mapping)
            local name = mapping_name(mapping, kind)
            local id = mapping_id(kind, index, name)
            local base = baseline[id]
            if base and is_pc_key(base.key) then
                local desired = overrides[id] or base.key
                if desired == "__DISABLED__" then desired = "None" end
                local current = mapping_key(mapping)
                if current ~= desired then
                    local set_ok, set_error = set_mapping_key(mapping, desired)
                    if not set_ok then error("failed mapping " .. id .. ": " .. tostring(set_error)) end
                    changed = changed + 1
                end
                if overrides[id] then
                    applied = applied + 1
                    missing[id] = nil
                end
            end
        end)

        local rebuild_ok, rebuild_error = pcall(function() settings:ForceRebuildKeymaps() end)
        if not rebuild_ok then error("ForceRebuildKeymaps failed: " .. tostring(rebuild_error)) end
        write_manifest(settings, overrides)

        local missing_count = 0
        for id in pairs(missing) do
            missing_count = missing_count + 1
            log("WARNING saved override no longer matches current build: " .. id)
        end
        log(string.format("apply #%d reason=%s overrides=%d changed=%d missing=%d", apply_number, tostring(reason), applied, changed, missing_count))
    end)

    if not ok then log("ERROR apply failed reason=" .. tostring(reason) .. ": " .. tostring(error_message)) end
    applying = false
end

local function request_apply(reason, delay_ms)
    if apply_pending then return end
    apply_pending = true
    ExecuteWithDelay(delay_ms or 0, function()
        local ok, queue_error = pcall(function()
            ExecuteInGameThread(function() apply_config(reason) end)
        end)
        if not ok then
            apply_pending = false
            log("ERROR failed to queue apply: " .. tostring(queue_error))
        end
    end)
end

log("loaded " .. VERSION .. "; offline editor config applies only during startup")
-- MouseMiddleSprint finishes its verified runtime rename after roughly two seconds.
-- Apply once at the login/title stage. Never rebuild mappings while a world is being played.
request_apply("startup-title-stage", 3500)
