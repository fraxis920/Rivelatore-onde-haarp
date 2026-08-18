
 
    .global geet_seed
    .global geet_rand8
    .global geet_rand16
    .global geet_rand32
 
    .section .data
    .align 4
geet_state8:
    .byte 0x42
    .align 2
geet_state16:
    .short 0xACE1
    .align 4
geet_state32:
    .long 0x12345678
 
    .section .text
 
    .align 4
    .type geet_seed,@function
geet_seed:
    entry   a1, 16
    bnez    a2, 1f
    movi    a2, 1
1:
    movi    a3, geet_state8
    s8i     a2, a3, 0
    movi    a3, geet_state16
    s8i     a2, a3, 0
    s8i     a2, a3, 1
    movi    a3, geet_state32
    s8i     a2, a3, 0
    s8i     a2, a3, 1
    s8i     a2, a3, 2
    s8i     a2, a3, 3
    retw.n
 
    .align 4
    .type geet_rand8,@function
geet_rand8:
    entry   a1, 16
    movi    a3, geet_state8
    l8ui    a4, a3, 0         
    extui   a5, a4, 0, 1      
    srli    a6, a4, 1          
    beqz    a5, 1f
    movi    a7, 0xB8
    xor     a6, a6, a7
1:
    s8i     a6, a3, 0
    extui   a2, a6, 0, 8     
    retw.n
 
    .align 4
    .type geet_rand16,@function
geet_rand16:
    entry   a1, 16
    movi    a3, geet_state16
    l8ui    a4, a3, 0        
    l8ui    a5, a3, 1         
    extui   a6, a4, 0, 1      
    slli    a7, a5, 8
    or      a7, a7, a4        
    srli    a7, a7, 1
    beqz    a6, 1f
    movi    a8, 0xB400        
    xor     a7, a7, a8
1:
    extui   a4, a7, 0, 8
    extui   a5, a7, 8, 8
    s8i     a4, a3, 0
    s8i     a5, a3, 1
    extui   a2, a7, 0, 16
    retw.n
 
    .align 4
    .type geet_rand32,@function
geet_rand32:
    entry   a1, 16
    movi    a3, geet_state32
    l8ui    a4, a3, 0          
    l8ui    a5, a3, 1         
    l8ui    a6, a3, 2        
    l8ui    a7, a3, 3         
    extui   a8, a4, 0, 1       
    slli    a9, a7, 24
    slli    a10, a6, 16
    or      a9, a9, a10
    slli    a10, a5, 8
    or      a9, a9, a10
    or      a9, a9, a4         
    srli    a9, a9, 1
    beqz    a8, 1f
    movi    a10, 0x80000057    
    xor     a9, a9, a10
1:
    extui   a4, a9, 0, 8
    extui   a5, a9, 8, 8
    extui   a6, a9, 16, 8
    extui   a7, a9, 24, 8
    s8i     a4, a3, 0
    s8i     a5, a3, 1
    s8i     a6, a3, 2
    s8i     a7, a3, 3
    mov     a2, a9
    retw.n
