# Hashing Policy

## Scope

Read this policy before changing a hash, checksum, fingerprint, deterministic
procedural sample, table mapping, seed derivation, or cryptographic hash.

## Approval Gate

**HASH-EXTEND-001:** The approved non-cryptographic set is closed. Stop before
implementing another mechanism and obtain explicit approval as described below.

## Hashing Policy

Managed `System.IO.Hashing.XxHash3` is the default and first choice for all
non-cryptographic hashing, including stable content fingerprints and
deterministic procedural sampling. Projects that use it declare an explicit
`System.IO.Hashing` package reference and call the managed static or incremental
API directly.

The only approved non-cryptographic alternatives are:

- `System.HashCode`, only for process-local CLR equality and collection hash
  codes where stable or deterministic output is not part of the contract.
- `AlvorKit.Hashing.TableHash`, only for mapping an already-formed integer key
  to a power-of-two table. Use `EpochIndex32` or `EpochIndex64` when their
  epoch-cleared key-to-slot lifecycle applies; they own the table hashing and
  are not another general-purpose hash.
- `AlvorKit.Hashing.AdditiveChecksum64`, only for a deliberately weak additive
  observation or sink where order-insensitivity and collision weakness are
  accepted, normally inside measured benchmark work. Never use it for
  identity, integrity, or correctness.

Never invent, copy, retain, or introduce another non-cryptographic hashing
system. This includes custom mixers, magic-prime combinations, FNV-like loops,
rolling hashes, lookup hashes, native bindings, P/Invoke backends, routing
wrappers, injected hash services, and game-local general-purpose hash
implementations. A hand-rolled hash is not an allowed fallback.

If managed XXH3 and the three alternatives above do not meet a requirement,
stop before changing code. Explain the exact semantics or measured performance
need, why each approved mechanism is unsuitable, the proposed owner and public
contract, and the tests and benchmarks that would justify an addition. Wait for
explicit user approval before implementing it.

Encode structured XXH3 inputs explicitly and portably. Use `BinaryPrimitives`
with a documented byte order, hash only the written span, and use `long` at the
seed domain boundary because the managed XXH3 API accepts `long`. Treat field
order, byte order, payload length, and seed derivation as part of any
deterministic-output contract. The C# `unchecked` keyword is forbidden; choose
types and explicit operations that make overflow and bit representation
semantics visible instead.

Cryptographic hashing is a separate requirement. Use the appropriate built-in
.NET cryptographic APIs, such as `SHA256`, `HMACSHA256`, or `IncrementalHash`.
Never substitute XXH3 for a cryptographic hash, and never implement custom
cryptography.
